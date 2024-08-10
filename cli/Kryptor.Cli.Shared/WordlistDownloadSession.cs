using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Downloader;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistDownloadSession : Session
    {
        string Id;
        Uri Uri;
        DownloadConfiguration Configuration;
        DownloadService DownloadService;

        public string CacheDir = Path.Combine(Program.Context.WordlistDirectory, "_cache");
        public string FileDir { get; private set; }

        public string PackPath { get; private set; }
        public string FilePath { get; private set; }

        public WordlistDownloadSession(Uri uri, string id)
        {
            Id = id;
            FileDir = Path.Combine(CacheDir, id);

            PackPath = Path.Combine(FileDir, "package.json");
            FilePath = Path.Combine(FileDir, "wordlist.txt");

            Uri = uri;

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

            DownloadService = new DownloadService(Configuration);
            DownloadService.DownloadProgressChanged += UpdateProgress;
            DownloadService.DownloadFileCompleted += SetEndStatus;
        }

        private void SetEndStatus(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                File.WriteAllText(PackPath, JsonConvert.SerializeObject(DownloadService.Package));
                Exception = e.Error;
            }
        }

        private void UpdateProgress(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            Description = $"{Id}: {Utilities.ConvertBytes(e.ReceivedBytesSize)}/{Utilities.ConvertBytes(e.TotalBytesToReceive)}";
            if (e.AverageBytesPerSecondSpeed > 1024 * 100)
            {
                Description += $"   {Utilities.ConvertBytes((long)e.AverageBytesPerSecondSpeed)}/s";
            }
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            Description = $"{Id}: Starting download";

            if (!Directory.Exists(CacheDir))
            {
                Directory.CreateDirectory(CacheDir);
            }

            if (!Directory.Exists(FileDir))
            {
                Directory.CreateDirectory(FileDir);
            }

            if (File.Exists(PackPath))
            {
                var package = JsonConvert.DeserializeObject<DownloadPackage>(File.ReadAllText(PackPath));
                File.Delete(PackPath);

                await DownloadService.DownloadFileTaskAsync(package, cancellationToken);
            }
            else
            {
                await DownloadService.DownloadFileTaskAsync(Uri.ToString(), FilePath, cancellationToken);
            }

            DownloadService.Dispose();

            if (Exception != null)
            {
                throw Exception;
            }

            return true;
        }
    }
}