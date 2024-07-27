using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSession : Session
    {
        public override double Progress { get; protected set; }

        public override string Description { get; protected set; }

        Kes kes;
        int hVerbose;
        string file;

        public EncryptionSession(KeyStore keyStore, CryptoProviderConfiguration configuration, int blockSize, int hVerbose, string file)
        {
            Description = "";

            kes = new Kes(keyStore, configuration, blockSize);
            kes.OnProgress += UpdateProgress;

            this.hVerbose = hVerbose;
            this.file = file;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Description = "Prepairing";

            await base.StartAsync(cancellationToken);

            if (!File.Exists(file))
            {
                Status = SessionStatus.Ended;
                EndReason = SessionEndReason.Skipped;
                return;
            }

            var header = CliHeader.Create();
            header.FileName = Path.GetFileName(file);
            header.Verbosity = (HeaderVerbosity)hVerbose;

            var sourceStream = File.OpenRead(file);
            var destFileName = Utilities.GetNewFileName(file, file + ".kef");
            var destStream = File.OpenWrite(destFileName);

            try
            {
                Description = destFileName;
                await kes.EncryptAsync(sourceStream, destStream, header, cancellationToken);
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

                Status = SessionStatus.Ended;
            }
        }

        private void UpdateProgress(double value)
        {
            Progress = value;
        }
    }
}