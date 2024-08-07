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

            KeyStore = LoadKeyStore(ks);
        }
    }
}