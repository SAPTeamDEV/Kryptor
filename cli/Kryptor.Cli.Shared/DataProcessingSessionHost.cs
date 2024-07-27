using System;
using System.IO;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingSessionHost : CliSessionHost
    {
        public int BlockSize { get; }
        public CryptoProviderConfiguration Configuration { get; }
        public KeyStore KeyStore { get; }
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

            if (File.Exists(keyStore))
            {
                var data = File.ReadAllBytes(keyStore);
                KeyStore = new KeyStore(data);
            }
            else if (TransformerToken.IsValid(keyStore))
            {
                var token = TransformerToken.Parse(keyStore);
                KeyStore = Utilities.GenerateKeyStoreFromToken(token);
            }
            else
            {
                throw new FileNotFoundException(keyStore);
            }

            Files = files;
        }
    }
}