using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }

        public EncryptionSessionHost(bool verbose, DataProcessingOptions options, int hVerbose) : base(verbose, options) => HeaderVerbosity = hVerbose;

        public override void Start(ClientContext context)
        {
            base.Start(context);

            foreach (string file in Files)
            {
                EncryptionSession session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, file);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}