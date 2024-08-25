using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSession : Session
    {
        private readonly Kes kes;
        private readonly int hVerbose;
        private readonly string file;

        public ClientHeader Header { get; private set; }

        public EncryptionSession(KeyStore keyStore, CryptoProviderConfiguration configuration, int blockSize, int hVerbose, string file)
        {
            Description = "";

            kes = new Kes(keyStore, configuration, blockSize);
            kes.ProgressChanged += UpdateProgress;

            this.hVerbose = hVerbose;
            this.file = file;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Description = "Prepairing";

            if (!File.Exists(file))
            {
                EndReason = SessionEndReason.Skipped;
                Description = "file not found";
                return false;
            }

            ClientHeader header = Header = CliHeader.Create();

            if (hVerbose > 1)
            {
                header.FileName = Path.GetFileName(file);
                header.GenerateSerial();
            }

            header.Verbosity = (HeaderVerbosity)hVerbose;

            FileStream sourceStream = File.OpenRead(file);
            string destFileName = Utilities.GetNewFileName(file, file + ".kef");
            FileStream destStream = File.OpenWrite(destFileName);

            try
            {
                Description = destFileName;
                await kes.EncryptAsync(sourceStream, destStream, header, cancellationToken);

                var encSessionHost = (EncryptionSessionHost)sessionHost;
                if (encSessionHost.UseKeyChain)
                {
                    Description = "Creating keychain";
                    encSessionHost.KeyChainCollection.Add(header, kes.Provider.KeyStore, TransformerToken.IsValid(encSessionHost.KeystoreString) ? encSessionHost.KeystoreString : null);
                    Description = destFileName;
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