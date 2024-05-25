using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MoreLinq;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides Transformed Parameters (TP) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key and iv.
    /// </summary>
    public sealed class TransformedParametersCryptoProvider : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "TransformedParameters";

        /// <inheritdoc/>
        public override bool RemoveHash
        {
            get
            {
                return false;
            }
            protected set
            {
                if (value)
                {
                    throw new NotImplementedException("This crypto provider does not supports remove hash feature.");
                }
            }
        }

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
        public TransformedParametersCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false) : base(keyStore, continuous, removeHash)
        {

        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await AesHelper.EncryptAesCbcAsync(chunk, Transformers.CreateKey(KeyStore, process), Transformers.CreateIV(KeyStore, process));
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await AesHelper.DecryptAesCbcAsync(cipher, Transformers.CreateKey(KeyStore, process), Transformers.CreateIV(KeyStore, process));
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
    }
}
