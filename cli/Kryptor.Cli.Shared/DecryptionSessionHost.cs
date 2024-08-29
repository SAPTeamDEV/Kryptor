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

            foreach (string file in Files)
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, file, OutputPath);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}