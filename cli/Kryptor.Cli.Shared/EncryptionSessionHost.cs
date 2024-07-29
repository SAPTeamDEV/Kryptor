using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }

        public EncryptionSessionHost(bool verbose, DataProcessingOptions options, int hVerbose) : base(verbose, options)
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

            ShowProgressMonitored(true).Wait();
        }
    }
}