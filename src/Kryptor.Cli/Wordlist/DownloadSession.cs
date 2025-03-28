using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Downloader;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class DownloadSession : Session
    {
        private readonly WordlistIndexEntry IndexEntry;
        private CancellationToken CancellationToken;
        private readonly DownloadConfiguration Configuration;
        private readonly DownloadService Downloader;
        private readonly JsonWorker JsonWorker;

        public FileInfo OutputFile;
        public FileInfo PackageFile;
        public string HashString;

        public DownloadSession(WordlistIndexEntry entry, DirectoryInfo outputPath) : this(entry, new FileInfo(Path.Combine(outputPath.FullName, entry.Id + ".txt")))
        {

        }

        public DownloadSession(WordlistIndexEntry entry, FileInfo output)
        {
            IndexEntry = entry;
            OutputFile = output;
            HashString = BitConverter.ToString(IndexEntry.Hash ?? Encoding.UTF8.GetBytes(IndexEntry.Uri.ToString()).Sha256()).Replace("-", "").ToLower();

            OutputFile.Directory.Create();
            PackageFile = new FileInfo(Path.Combine(OutputFile.Directory.FullName, $"package-{IndexEntry.Id}-{HashString.Substring(0, 6)}.json"));

            JsonSerializerOptions jOptions = new JsonSerializerOptions()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            JsonWorker = new JsonWorker(null, SourceGenerationPackageContext.Default);

            Description = $"{entry.Id}: Initializing download";

            Configuration = new DownloadConfiguration()
            {
                // file parts to download, the default value is 1
                ChunkCount = 4,
                // the maximum number of times to fail
                MaxTryAgainOnFailover = 8,
                // release memory buffer after each 50 MB
                MaximumMemoryBufferBytes = 1024 * 1024 * 64,
                // download parts of the file as parallel or not. The default value is false
                ParallelDownload = true,
                // number of parallel downloads. The default value is the same as the chunk count
                ParallelCount = 2,
                // timeout (millisecond) per stream block reader, default values is 1000
                Timeout = 5000,
                // clear package chunks data when download completed with failure, default value is false
                ClearPackageOnCompletionWithFailure = false,
                // minimum size of chunking to download a file in multiple parts, the default value is 512
                MinimumSizeOfChunking = 1024 * 1024,
                // Before starting the download, reserve the storage space of the file as file size, the default value is false
                ReserveStorageSpaceBeforeStartingDownload = true,
                // config and customize request headers
                RequestConfiguration =
                {
                    Accept = "*/*",
                    KeepAlive = true, // default value is false
                    ProtocolVersion = HttpVersion.Version11, // default value is HTTP 1.1
                    UseDefaultCredentials = false,
                    // your custom user agent or your_app_name/app_version.
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                }
            };

            Downloader = new DownloadService(Configuration);
            Downloader.DownloadProgressChanged += UpdateProgress;
            Downloader.DownloadFileCompleted += SetEndStatus;

            Description = $"{entry.Id}: Ready";
        }

        private void SetEndStatus(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                File.WriteAllText(PackageFile.FullName, JsonWorker.ToJson(Downloader.Package));
                Exception = e.Error;
            }
            else
            {
                IndexEntry.Size = Downloader.Package.TotalFileSize;

                Progress = -1;

                Description = $"{IndexEntry.Id}: Verifying file";

                VerifyHash(File.OpenRead(Downloader.Package.FileName), CancellationToken);

                if (IndexEntry.Compressed)
                {
                    Description = $"{IndexEntry.Id}: Extracting file";
                    string fileName = new Request(IndexEntry.Uri.ToString()).GetFileName().Result;

                    if (fileName.ToLower().EndsWith(".7z"))
                    {
                        SevenZipArchive reader = SevenZipArchive.Open(Downloader.Package.FileName);
                        reader.Entries.First().WriteToFile(OutputFile.FullName);
                        reader.Dispose();
                    }
                    else
                    {
                        IReader reader = ReaderFactory.Open(File.OpenRead(Downloader.Package.FileName));
                        reader.MoveToNextEntry();
                        reader.WriteEntryTo(OutputFile);
                        reader.Dispose();
                    }
                }
                else
                {
                    if (File.Exists(OutputFile.FullName))
                    {
                        OutputFile.Delete();
                    }

                    File.Move(Downloader.Package.FileName, OutputFile.FullName);
                }

                if (File.Exists(Downloader.Package.FileName))
                {
                    File.Delete(Downloader.Package.FileName);
                }

                Progress = 100;

                Description = $"{IndexEntry.Id}: Download completed";
            }
        }

        private void UpdateProgress(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            Description = $"{IndexEntry.Id}: {Utilities.ConvertBytes(e.ReceivedBytesSize)}/{Utilities.ConvertBytes(e.TotalBytesToReceive)}";
            if (e.AverageBytesPerSecondSpeed > 1024 * 100)
            {
                Description += $"   {Utilities.ConvertBytes((long)e.AverageBytesPerSecondSpeed)}/s";
            }
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            Description = $"{IndexEntry.Id}: Starting download";

            if (File.Exists(PackageFile.FullName))
            {
                DownloadPackage package = JsonWorker.ReadJson<DownloadPackage>(File.ReadAllText(PackageFile.FullName));
                PackageFile.Delete();

                await Downloader.DownloadFileTaskAsync(package, cancellationToken);
            }
            else
            {
                string dest = Utilities.EnsureDirectoryExists(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

                string tempFile = Path.Combine(dest, HashString);

                await Downloader.DownloadFileTaskAsync(IndexEntry.Uri.OriginalString, tempFile, cancellationToken);
            }

            Downloader.Dispose();

            return Exception != null ? throw Exception : true;
        }

        public void DeleteCache()
        {
            if (File.Exists(OutputFile.FullName))
            {
                OutputFile.Delete();
            }

            if (OutputFile.Directory.GetFiles().Length == 0)
            {
                OutputFile.Directory.Delete();
            }
        }

        private void VerifyHash(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                byte[] hash = stream.Sha256();

                if (IndexEntry.Hash == null)
                {
                    IndexEntry.Hash = hash;
                }
                else
                {
                    if (!IndexEntry.Hash.SequenceEqual(hash))
                    {
                        throw new InvalidDataException("File is corrupted");
                    }
                }
            }
            catch (InvalidDataException)
            {
                stream?.Dispose();
                throw;
            }
            catch
            {
                // Ignore hash errors
            }
            finally
            {
                stream?.Dispose();
            }
        }
    }

    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(DownloadPackage))]
    internal partial class SourceGenerationPackageContext : JsonSerializerContext
    {
    }
}