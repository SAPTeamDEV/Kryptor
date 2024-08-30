using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DecryptionSessionHost : DataProcessingSessionHost
    {
        public DecryptionSessionHost(GlobalOptions globalOptions, DataProcessingOptions options) : base(globalOptions, options)
        {

        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            foreach (var entry in Files)
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, entry.Key, entry.Value);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}