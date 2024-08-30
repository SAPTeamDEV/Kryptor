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

            foreach (var entry in Files)
            {
                EncryptionSession session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, entry.Key, entry.Value);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();

            if (UseKeyChain)
            {
                DebugLog("Updating KeyChain data");
                KeyChainCollection.Save();
            }
        }
    }
}