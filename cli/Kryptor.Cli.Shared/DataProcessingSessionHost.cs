using System;
using System.IO;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingSessionHost : CliSessionHost
    {
        public int BlockSize { get; }

        public CryptoProviderConfiguration Configuration { get; }

        public KeyStore KeyStore { get; private set; }
        string ks;

        public string[] Files {  get; }

        public DataProcessingSessionHost(bool verbose, DataProcessingOptions options) : base(verbose)
        {
            BlockSize = options.BlockSize;

            Configuration = new CryptoProviderConfiguration()
            {
                Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(options.Provider),
                Continuous = options.Continuous,
                RemoveHash = options.RemoveHash,
                DynamicBlockProccessing = options.DynamicBlockProcessing,
            };

            Files = options.Files;

            ks = options.KeyStore;
        }

        public override void Start()
        {
            base.Start();

            if (File.Exists(ks))
            {
                DebugLog($"Keystore file: {ks}");
                var session = new KeyStoreFileLoadSession(ks);
                NewSession(session, true);
                ShowProgressMonitored(false).Wait();
                KeyStore = session.KeyStore;
                
            }
            else if (TransformerToken.IsValid(ks))
            {
                DebugLog($"Transformer token: {ks}");
                var token = TransformerToken.Parse(ks);

                if (Verbose)
                {
                    var tranformer = Transformers.GetTranformer(token);
                    DebugLog($"Generating keystore with {token.KeySize} keys using {tranformer.GetType().Name}");
                }

                var session = new KeyStoreTokenLoadSession(token);
                NewSession(session, true);
                ShowProgressMonitored(false).Wait();
                KeyStore = session.KeyStore;
            }
            else
            {
                throw new FileNotFoundException(ks);
            }

            DebugLog($"Keystore fingerprint: {KeyStore.Fingerprint.FormatFingerprint()}");
        }
    }
}