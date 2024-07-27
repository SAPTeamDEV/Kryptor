using System;
using System.IO;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingSessionHost : CliSessionHost
    {
        public int BlockSize { get; }
        public string Provider { get; }
        public bool Continuous { get; }
        public bool RemoveHash { get; }
        public bool DynamicBlockProcessing { get; }
        public KeyStore KeyStore { get; }
        public string[] Files {  get; }

        public DataProcessingSessionHost(int blockSize, string provider, bool continuous, bool removeHash, bool dbp, string keyStore, string[] files)
        {
            BlockSize = blockSize;

            Provider = CryptoProviderFactory.GetRegisteredCryptoProviderId(provider);

            Continuous = continuous;
            RemoveHash = removeHash;
            DynamicBlockProcessing = dbp;

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