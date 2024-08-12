using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DecryptionSessionHost : DataProcessingSessionHost
    {
        public DecryptionSessionHost(bool verbose, DataProcessingOptions options) : base(verbose, options)
        {

        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            foreach (string file in Files)
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, file);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}