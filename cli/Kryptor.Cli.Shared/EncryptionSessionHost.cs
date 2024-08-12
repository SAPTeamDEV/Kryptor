namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }

        public EncryptionSessionHost(bool verbose, DataProcessingOptions options, int hVerbose) : base(verbose, options) => HeaderVerbosity = hVerbose;

        public override void Start()
        {
            base.Start();

            foreach (string file in Files)
            {
                EncryptionSession session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, file);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}