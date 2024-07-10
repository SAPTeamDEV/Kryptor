using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Transformed Key (TK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key.
    /// </summary>
    public sealed class TransformedKey : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "TransformedKey";

        /// <inheritdoc/>
        public override bool RemoveHash
        {
            get => false;
            protected set
            {
                if (value)
                {
                    throw new NotImplementedException("This crypto provider does not supports remove hash feature.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformedKey"/> class.
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
        public TransformedKey(KeyStore keyStore, bool continuous = false, bool removeHash = false, bool dynamicBlockProccessing = false) : base(keyStore, continuous, removeHash, dynamicBlockProccessing)
        {

        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await AesHelper.EncryptAesEcbAsync(chunk, Transformers.CreateKey(KeyStore, process));
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await AesHelper.DecryptAesEcbAsync(cipher, Transformers.CreateKey(KeyStore, process));
        }

        /// <inheritdoc/>
        protected internal override void UpdateHeader(Header header)
        {
            base.UpdateHeader(header);

            if ((int)header.DetailLevel > 2)
            {
                header.CryptoType = CryptoTypes.TK;
            }
        }
    }
}
