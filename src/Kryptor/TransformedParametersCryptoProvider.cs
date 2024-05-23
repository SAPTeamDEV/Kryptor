using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using EnsureThat;

using MoreLinq;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides Transformed Parameters (TP) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key and iv.
    /// </summary>
    public class TransformedParametersCryptoProvider : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformedParametersCryptoProvider"/> class.
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
        public TransformedParametersCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false)
        {
            KeyStore = keyStore;
            Continuous = continuous;
            RemoveHash = removeHash;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await MixedVectorCryptoProvider.EncryptAsync(chunk, CreateKey(process), CreateIV(process));
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await MixedVectorCryptoProvider.DecryptAsync(cipher, CreateKey(process), CreateIV(process));
        }

        /// <inheritdoc/>
        protected internal override void UpdateHeader(Header header)
        {
            base.UpdateHeader(header);

            if ((int)header.DetailLevel > 2)
            {
                header.CryptoType = CryptoTypes.TP;
            }
        }

        private byte[] CreateKey(CryptoProcess process)
        {
            int seed = Transformers.ToAbsInt32(process.BlockHash, process.BlockIndex + process.ChunkIndex);
            var sets = Transformers.Pick(KeyStore.Keys, (seed % 8) + 1, seed).SelectMany(x => x);

            var mixed = Transformers.Mix(seed, sets);
            var key = Transformers.Pick(mixed, 32, seed);

            return key.ToArray();
        }

        byte[] CreateIV(CryptoProcess process)
        {
            return Transformers.Pick(KeyStore.Keys, 1, (process.BlockHash[5] % (process.BlockHash[19] + 4)) - process.ChunkIndex).First().Take(16).ToArray();
        }
    }
}
