using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Mixed Vector (MV) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a different key and a mixed iv Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
    /// </summary>
    public sealed class MixedVector : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MixedVector"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize the crypto provider
        /// </param>
        public MixedVector(KeyStore keyStore, CryptoProviderConfiguration configuration = null) : base(keyStore, configuration)
        {

        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await AesHelper.EncryptAesCbcAsync(chunk, KeyStore[process.ChunkIndex], DynamicEncryption.CreateMixedIV(KeyStore, process));
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await AesHelper.DecryptAesCbcAsync(cipher, KeyStore[process.ChunkIndex], DynamicEncryption.CreateMixedIV(KeyStore, process));
        }
    }
}
