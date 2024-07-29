using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DecryptionSession : Session
    {
        public override double Progress { get; protected set; }
        public override string Description { get; protected set; }

        KeyStore keyStore;
        CryptoProviderConfiguration configuration;
        int blockSize;
        string file;

        public DecryptionSession(KeyStore keyStore, CryptoProviderConfiguration configuration, int blockSize, string file)
        {
            Description = "";

            this.keyStore = keyStore;
            this.configuration = configuration;
            this.blockSize = blockSize;
            this.file = file;
        }

        protected async override Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(file))
            {
                EndReason = SessionEndReason.Skipped;
                Description = "file not found";
                return false;
            }

            Description = "Reading header";

            var sourceStream = File.OpenRead(file);
            var header = Header.ReadHeader<CliHeader>(sourceStream);

            var destFileName = "decrypted file.dec";

            int hVerbose = (int)header.Verbosity;

            int bs = blockSize;
            CryptoProviderConfiguration config = configuration;

            if (hVerbose > 0)
            {
                if (header.Version != Kes.Version)
                {
                    if (header.ClientName != null)
                    {
                        Description = $"You must use {header.ClientName} v{header.ClientVersion}";
                    }
                    else
                    {
                        Description = $"You must use a client with engine version {header.EngineVersion} or api version {header.Version}";
                    }

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

            Kes kes = new Kes(keyStore, config, bs);
            kes.OnProgress += UpdateProgress;

            destFileName = Utilities.GetNewFileName(file, destFileName);
            var destStream = File.OpenWrite(destFileName);

            try
            {
                Description = destFileName;
                await kes.DecryptAsync(sourceStream, destStream, cancellationToken);
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

        private void UpdateProgress(double value)
        {
            Progress = value;
        }
    }
}