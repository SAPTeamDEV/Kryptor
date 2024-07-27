using System;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }

        public EncryptionSessionHost(int blockSize, string provider, bool continuous, bool removeHash, bool dbp, string keyStore, string[] files, int hVerbose) : base(blockSize, provider, continuous, removeHash, dbp, keyStore, files)
        {
            HeaderVerbosity = hVerbose;
        }

        public override void Start()
        {
            base.Start();

            var configuration = new CryptoProviderConfiguration()
            {
                Id = Provider,
                Continuous = Continuous,
                RemoveHash = RemoveHash,
                DynamicBlockProccessing = DynamicBlockProcessing,
            };

            foreach (var file in Files)
            {
                var session = new EncryptionSession(KeyStore, configuration, BlockSize, HeaderVerbosity, file);
                NewSession(session);
            }

            var pTask = ShowProgress();
            MonitorTask(pTask);
            pTask.Wait();
        }
    }
}