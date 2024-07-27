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

            foreach (var file in Files)
            {
                var session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, file);
                NewSession(session);
            }

            ShowProgressMonitored().Wait();
        }
    }
}