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

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (!File.Exists(file))
            {
                Status = SessionStatus.Ended;
                EndReason = SessionEndReason.Skipped;
                Description = "file not found";
                return;
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

                    Status = SessionStatus.Ended;
                    EndReason = SessionEndReason.Failed;
                    return;
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
                EndReason = SessionEndReason.Completed;
            }
            catch (OperationCanceledException ocex)
            {
                EndReason = SessionEndReason.Cancelled;
                Exception = ocex;
            }
            catch (Exception ex)
            {
                Description = $"{ex.GetType().Name}: {ex.Message}";
                EndReason = SessionEndReason.Failed;
                Exception = ex;
            }
            finally
            {
                sourceStream.Close();
                destStream.Close();

                if (EndReason != SessionEndReason.Completed)
                {
                    File.Delete(destFileName);
                }

                Timer.Stop();
                Status = SessionStatus.Ended;
            }

        }

        private void UpdateProgress(double value)
        {
            Progress = value;
        }
    }
}