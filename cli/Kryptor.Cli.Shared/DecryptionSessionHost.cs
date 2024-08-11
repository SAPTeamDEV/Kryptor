namespace SAPTeam.Kryptor.Cli
{
    public class DecryptionSessionHost : DataProcessingSessionHost
    {
        public DecryptionSessionHost(bool verbose, DataProcessingOptions options) : base(verbose, options)
        {

        }

        public override void Start()
        {
            base.Start();

            foreach (string file in Files)
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, file);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}