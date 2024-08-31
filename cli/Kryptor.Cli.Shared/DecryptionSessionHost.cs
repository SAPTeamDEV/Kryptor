using SAPTeam.Kryptor.Client;

using SharpCompress.Common;

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

            EnumerateFiles((filePath, outputPath) =>
            {
                DecryptionSession session = new DecryptionSession(KeyStore, Configuration, BlockSize, filePath, outputPath);
                NewSession(session, autoStart: false, sessionGroup: sessionGroup);
            });

            Container.StartQueuedSessions(sessionGroup);
            ShowProgressMonitored(sessionGroup).Wait();
        }
    }
}