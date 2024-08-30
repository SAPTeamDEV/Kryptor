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

            var sessionGroup = new SessionGroup();

            Parallel.ForEach(Files, entry =>
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, entry.Key, entry.Value);
                sessionGroup.Add(session);
                NewSession(session, autoStart: false);
            });

            Container.StartQueuedSessions();
            ShowProgressMonitored(sessionGroup).Wait();
        }
    }
}