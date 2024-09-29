using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.Cli
{
    public class FileSaveSession : Session
    {
        private string filePath;

        public string FilePath
        {
            get => filePath;

            set => filePath = string.IsNullOrEmpty(value) ? value : Path.GetFullPath(value);
        }

        public byte[] Data { get; set; }

        public FileSaveSession(string filePath, byte[] data)
        {
            FilePath = filePath;
            Data = data;

            if (!string.IsNullOrEmpty(FilePath))
            {
                Description = FilePath;
            }
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Description = FilePath;

            try
            {
                await WriteAsync(cancellationToken);

                return true;
            }
            catch
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }

                throw;
            }
        }

        private async Task WriteAsync(CancellationToken cancellationToken)
        {
            using (FileStream f = File.OpenWrite(FilePath))
            {
                double steps = 1.0 / Data.Length * 100;
                int wroteBytes = 0;

                while (wroteBytes < Data.Length)
                {
                    int chunk = Math.Min(Data.Length - wroteBytes, 8192);
                    await AsyncCompat.WriteAsync(f, Data, 0, chunk, cancellationToken);
                    wroteBytes += chunk;
                    Progress = wroteBytes * steps;
                }
                
                f.Flush();
            }
        }
    }
}