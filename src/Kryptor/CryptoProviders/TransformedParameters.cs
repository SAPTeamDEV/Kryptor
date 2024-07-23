using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Transformed Parameters (TP) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key and iv.
    /// </summary>
    public sealed class TransformedParameters : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformedParameters"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize the crypto provider
        /// </param>
        public TransformedParameters(KeyStore keyStore, CryptoProviderConfiguration configuration = null) : base(keyStore, configuration)
        {
            if (Configuration.RemoveHash)
            {
                throw new NotImplementedException("This crypto provider does not supports remove hash feature.");
            }
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken)
        {
            return await AesHelper.EncryptAesCbcAsync(chunk, Transformers.CreateKey(KeyStore, process), Transformers.CreateIV(KeyStore, process), cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process, CancellationToken cancellationToken)
        {
            return await AesHelper.DecryptAesCbcAsync(cipher, Transformers.CreateKey(KeyStore, process), Transformers.CreateIV(KeyStore, process), cancellationToken);
        }
    }
}
