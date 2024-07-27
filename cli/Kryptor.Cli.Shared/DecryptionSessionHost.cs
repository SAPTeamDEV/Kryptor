namespace SAPTeam.Kryptor.Cli
{
    public class DecryptionSessionHost : DataProcessingSessionHost
    {
        public DecryptionSessionHost(int blockSize, string provider, bool continuous, bool removeHash, bool dbp, string keyStore, string[] files) : base(blockSize, provider, continuous, removeHash, dbp, keyStore, files)
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