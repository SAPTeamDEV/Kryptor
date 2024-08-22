using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSession : Session
    {
        private static CryptoRandom crng = new CryptoRandom();

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

                string[] serial = new string[5];
                for (int i = 0; i < 5; i++)
                {
                    serial[i] = crng.Next(0x1869F).ToString("D5");
                }

                header.Serial = string.Join("-", serial);
            }

            header.Verbosity = (HeaderVerbosity)hVerbose;

            FileStream sourceStream = File.OpenRead(file);
            string destFileName = Utilities.GetNewFileName(file, file + ".kef");
            FileStream destStream = File.OpenWrite(destFileName);

            try
            {
                Description = destFileName;
                await kes.EncryptAsync(sourceStream, destStream, header, cancellationToken);
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