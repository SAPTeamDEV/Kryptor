using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSession : Session
    {
        private readonly Kes kes;
        private readonly int hVerbose;
        private readonly string file;
        private readonly string outputPath;

        public ClientHeader Header { get; private set; }

        public override string Name => $"{base.Name}[{file}]";

        public EncryptionSession(KeyStore keyStore, CryptoProviderConfiguration configuration, int blockSize, int hVerbose, string file, string outputPath)
        {
            Description = file;

            kes = new Kes(keyStore, configuration, blockSize);
            kes.ProgressChanged += UpdateProgress;

            this.hVerbose = hVerbose;
            this.file = file;
            this.outputPath = outputPath;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            EncryptionSessionHost encSessionHost = (EncryptionSessionHost)sessionHost;

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

            string destName;
            if (encSessionHost.Obfuscate)
            {
                byte[] buffer = new byte[12];
                CryptoRandom.Instance.Generate(buffer);
                destName = BitConverter.ToString(buffer).Replace("-", "").ToLower();
            }
            else
            {
                destName = Path.GetFileName(file);
            }

            string destFileName = Utilities.GetNewFileName(outputPath, $"{destName}.kef");

            FileStream destStream = File.OpenWrite(destFileName);

            try
            {
                await kes.EncryptAsync(sourceStream, destStream, header, cancellationToken);
                destStream.Flush();

                if (encSessionHost.UseKeyChain)
                {
                    Description = "Creating keychain";
                    encSessionHost.KeyChainCollection.Add(header, kes.Provider.KeyStore, TransformerToken.IsValid(encSessionHost.KeystoreString) ? encSessionHost.KeystoreString : null);
                    Description = file;
                }

                if (encSessionHost.Verbose)
                {
                    Messages.Add($"Encrypted to {destFileName}");
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