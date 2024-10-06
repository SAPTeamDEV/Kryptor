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

            SessionGroup sessionGroup = Container.SetSessionGroup(new SessionGroup());

            EnumerateFiles((filePath, outputPath) =>
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, filePath, outputPath);
                NewSession(session, autoStart: false);
            });

            Container.StartQueuedSessions();
            GetSmartProgress(sessionGroup).Wait();
        }
    }
}