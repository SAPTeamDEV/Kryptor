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

            foreach (var file in Files)
            {
                var session = new DecryptionSession(KeyStore, Configuration, BlockSize, file);
                NewSession(session);
            }

            ShowProgressMonitored().Wait();
        }
    }
}