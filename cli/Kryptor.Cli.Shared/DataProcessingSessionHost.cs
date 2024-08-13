using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingSessionHost : CliSessionHost
    {
        public int BlockSize { get; }

        public CryptoProviderConfiguration Configuration { get; }

        public KeyStore KeyStore { get; private set; }

        private readonly string ks;

        public string[] Files { get; }

        public DataProcessingSessionHost(GlobalOptions globalOptions, DataProcessingOptions options) : base(globalOptions)
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

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStore = LoadKeyStore(ks);
        }
    }
}