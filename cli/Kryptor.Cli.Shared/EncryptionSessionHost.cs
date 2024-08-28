using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }
        public string KeyChainPath { get; }
        public bool UseKeyChain { get; }
        public KeyChainCollection KeyChainCollection;

        public EncryptionSessionHost(GlobalOptions globalOptions, DataProcessingOptions options, int hVerbose, string keyChainPath) : base(globalOptions, options)
        {
            HeaderVerbosity = hVerbose;
            KeyChainPath = keyChainPath;
            UseKeyChain = hVerbose > 1 && !string.IsNullOrEmpty(keyChainPath);
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (UseKeyChain)
            {
                KeyChainCollection = new KeyChainCollection(KeyChainPath);
            }

            foreach (string file in Files)
            {
                EncryptionSession session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, file);
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