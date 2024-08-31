using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DecryptionSession : Session
    {
        private readonly KeyStore keyStore;
        private readonly CryptoProviderConfiguration configuration;
        private readonly int blockSize;
        private readonly string file;
        private readonly string outputPath;

        public override string Name => $"{base.Name}[{file}]";

        public DecryptionSession(KeyStore keyStore, CryptoProviderConfiguration configuration, int blockSize, string file, string outputPath)
        {
            Description = file;

            this.keyStore = keyStore;
            this.configuration = configuration;
            this.blockSize = blockSize;
            this.file = file;
            this.outputPath = outputPath;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            if (!File.Exists(file))
            {
                EndReason = SessionEndReason.Skipped;
                Description = "file not found";
                return false;
            }

            FileStream sourceStream = File.OpenRead(file);
            CliHeader header = Header.ReadHeader<CliHeader>(sourceStream);

            string destFileName = "decrypted file.dec";

            int hVerbose = (int)header.Verbosity;

            int bs = blockSize;
            CryptoProviderConfiguration config = configuration;

            if (hVerbose > 0)
            {
                if (header.Version < Kes.MinimumSupportedVersion || header.Version > Kes.Version)
                {
                    Description = header.ClientName != null
                        ? $"You must use {header.ClientName} v{header.ClientVersion}"
                        : $"You must use a client with engine version {header.EngineVersion} or api version {header.Version}";

                    EndReason = SessionEndReason.Failed;
                    return false;
                }

                if (hVerbose > 1)
                {
                    bs = header.BlockSize;

                    if (header.FileName != null)
                    {
                        destFileName = header.FileName;
                    }
                }

                if (hVerbose > 2)
                {
                    config = header.Configuration;
                }
            }
            else
            {
                Messages.Add("Warning: Cannot find any valid header in this file");
            }

            Kes kes = new Kes(keyStore, config, bs);
            kes.ProgressChanged += UpdateProgress;

            destFileName = Utilities.GetNewFileName(outputPath, destFileName);
            FileStream destStream = File.OpenWrite(destFileName);

            try
            {
                Description = file;
                await kes.DecryptAsync(sourceStream, destStream, cancellationToken);

                if (sessionHost.Verbose)
                {
                    Messages.Add($"Decrypted to {destFileName}");
                }

                return true;
            }
            catch (Exception)
            {
                sourceStream.Close();
                destStream.Close();
                File.Delete(destFileName);
                throw;
            }
        }

        private void UpdateProgress(object sender, double value) => Progress = value;
    }
}