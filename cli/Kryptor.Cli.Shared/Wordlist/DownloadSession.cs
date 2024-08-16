using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Downloader;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

using SharpCompress.Archives;
using SharpCompress.Readers;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class DownloadSession : Session
    {
        private WordlistIndexEntryV2 IndexEntry;
        private CancellationToken CancellationToken;
        private readonly DownloadConfiguration Configuration;
        private readonly DownloadService Downloader;

        public FileInfo OutputFile;
        public FileInfo PackageFile;

        public DownloadSession(WordlistIndexEntryV2 entry, DirectoryInfo outputPath) : this(entry, new FileInfo(Path.Combine(outputPath.FullName, entry.Id + ".txt")))
        {

        }

        public DownloadSession(WordlistIndexEntryV2 entry, FileInfo output)
        {
            IndexEntry = entry;
            OutputFile = output;

            OutputFile.Directory.Create();
            PackageFile = new FileInfo(Path.Combine(OutputFile.Directory.FullName, $"package-{IndexEntry.Id}.json"));

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
                File.WriteAllText(PackageFile.FullName, JsonConvert.SerializeObject(Downloader.Package));
                Exception = e.Error;
            }
            else
            {
                IndexEntry.Size = Downloader.Package.TotalFileSize;

                Progress = -1;

                Description = $"{IndexEntry.Id}: Verifying file";

                VerifyHash(File.OpenRead(Downloader.Package.FileName), CancellationToken).Wait();

                if (IndexEntry.Compressed)
                {
                    Description = $"{IndexEntry.Id}: Extracting file";

                    if (Downloader.Package.FileName.ToLower().EndsWith(".7z"))
                    {
                        //var destStream = OutputFile.Create();
                        //var svenZipReader = new SevenZipArchive(Downloader.Package.FileName);
                        //svenZipReader.Entries.ForEach(x => x.Extract(destStream));
                        //svenZipReader.Dispose();
                        //destStream.Dispose();

                        var reader = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(Downloader.Package.FileName);
                        reader.Entries.First().WriteToFile(OutputFile.FullName);
                        reader.Dispose();
                    }
                    else
                    {
                        var reader = ReaderFactory.Open(File.OpenRead(Downloader.Package.FileName));
                        reader.MoveToNextEntry();
                        reader.WriteEntryTo(OutputFile);
                        reader.Dispose();
                    }
                }
                else
                {
                    if (OutputFile.Exists)
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

            if (PackageFile.Exists)
            {
                DownloadPackage package = JsonConvert.DeserializeObject<DownloadPackage>(File.ReadAllText(PackageFile.FullName));
                PackageFile.Delete();

                await Downloader.DownloadFileTaskAsync(package, cancellationToken);
            }
            else
            {
                var dest = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

                if (!dest.Exists)
                {
                    dest.Create();
                }

                await Downloader.DownloadFileTaskAsync(IndexEntry.Uri.OriginalString, dest, cancellationToken);
            }

            Downloader.Dispose();

            return Exception != null ? throw Exception : true;
        }

        public void DeleteCache()
        {
            if (OutputFile.Exists)
            {
                OutputFile.Delete();
            }

            if (OutputFile.Directory.GetFiles().Length == 0)
            {
                OutputFile.Directory.Delete();
            }
        }

        private async Task VerifyHash(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                byte[] hash = buffer.Sha256();

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
}