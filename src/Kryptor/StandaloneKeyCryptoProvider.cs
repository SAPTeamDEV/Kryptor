using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using EnsureThat;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides Standalone Key (SK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
    /// </summary>
    public sealed class StandaloneKeyCryptoProvider : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "StandaloneKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneKeyCryptoProvider"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="continuous">
        /// Whether to use continuous encryption method.
        /// </param>
        /// <param name="removeHash">
        /// Whether to remove block hashes.
        /// </param>
        public StandaloneKeyCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false) : base(keyStore, continuous, removeHash)
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

        /// <inheritdoc/>
        protected internal override void UpdateHeader(Header header)
        {
            base.UpdateHeader(header);

            if ((int)header.DetailLevel > 2)
            {
                header.CryptoType = CryptoTypes.SK;
            }
        }
    }
}
