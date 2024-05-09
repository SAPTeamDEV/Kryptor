using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides Standalone Key (SK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
    /// </summary>
    public class StandaloneKeyCryptoProvider : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneKeyCryptoProvider"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="continuous">
        /// Whether to use continuous encryption method.
        /// </param>
        public StandaloneKeyCryptoProvider(KESKeyStore keyStore, bool continuous = false)
        {
            KeyStore = keyStore;
            Continuous = continuous;
        }

        /// <inheritdoc/>
        protected async override Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk)
        {
            return await AESEncryptProvider.EncryptAsync(chunk, KeyStore[index++]);
        }

        /// <inheritdoc/>
        protected async override Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher)
        {
            return await AESEncryptProvider.DecryptAsync(cipher, KeyStore[index++]);
        }
    }
}
