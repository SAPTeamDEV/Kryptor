using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Standalone Key (SK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
    /// </summary>
    public sealed class StandaloneKey : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneKey"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize the crypto provider
        /// </param>
        public StandaloneKey(KeyStore keyStore, CryptoProviderConfiguration configuration = null) : base(keyStore, configuration)
        {

        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await AesHelper.EncryptAesEcbAsync(chunk, KeyStore[process.ChunkIndex]);
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await AesHelper.DecryptAesEcbAsync(cipher, KeyStore[process.ChunkIndex]);
        }
    }
}
