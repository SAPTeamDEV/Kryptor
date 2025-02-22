using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }
        public string KeyChainPath { get; }
        public bool UseKeyChain { get; }
        public KeyChainCollection KeyChainCollection { get; private set; }
        public bool Obfuscate { get; }

        public EncryptionSessionHost(GlobalOptions globalOptions, DataProcessingOptions options, int hVerbose, string keyChainPath, bool obfuscate) : base(globalOptions, options)
        {
            HeaderVerbosity = hVerbose;
            KeyChainPath = keyChainPath;
            UseKeyChain = hVerbose > 1 && !string.IsNullOrEmpty(keyChainPath);
            Obfuscate = obfuscate;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (UseKeyChain)
            {
                KeyChainCollection = new KeyChainCollection(KeyChainPath);
            }

            SessionGroup sessionGroup = Container.SetSessionGroup(new SessionGroup());

            EnumerateFiles((filePath, outputPath) =>
            {
                EncryptionSession session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, filePath, outputPath);
                NewSession(session, autoStart: false);
            });

            Container.StartQueuedSessions();
            GetSmartProgress(sessionGroup).Wait();

            if (UseKeyChain)
            {
                DebugLog("Updating KeyChain data");
                KeyChainCollection.Save();
            }
        }
    }
}