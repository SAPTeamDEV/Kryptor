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

        public DataProcessingSessionHost(int blockSize, string provider, bool continuous, bool removeHash, bool dbp, string keyStore, string[] files)
        {
            BlockSize = blockSize;

            Configuration = new CryptoProviderConfiguration()
            {
                Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(provider),
                Continuous = continuous,
                RemoveHash = removeHash,
                DynamicBlockProccessing = dbp,
            };

            Files = files;

            ks = keyStore;
        }

        public override void Start()
        {
            base.Start();

            Console.WriteLine("Loading keystore");

            if (File.Exists(ks))
            {
                var data = File.ReadAllBytes(ks);
                KeyStore = new KeyStore(data);
            }
            else if (TransformerToken.IsValid(ks))
            {
                var token = TransformerToken.Parse(ks);
                KeyStore = Utilities.GenerateKeyStoreFromToken(token);
            }
            else
            {
                throw new FileNotFoundException(ks);
            }
        }
    }
}