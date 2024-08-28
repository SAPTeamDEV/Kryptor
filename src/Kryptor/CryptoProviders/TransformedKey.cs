using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Transformed Key (TK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key.
    /// </summary>
    public sealed class TransformedKey : CryptoProvider
    {
        /// <summary>
        /// Creates an empty crypto provider.
        /// </summary>
        public TransformedKey()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformedKey"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize the crypto provider
        /// </param>
        public TransformedKey(KeyStore keyStore, CryptoProviderConfiguration configuration = null)
        {
            ApplyHeader(keyStore, configuration);
        }

        /// <inheritdoc/>
        protected override void ApplyHeader(KeyStore keyStore, CryptoProviderConfiguration configuration = null)
        {
            base.ApplyHeader(keyStore, configuration);

            if (Configuration.RemoveHash)
            {
                throw new NotImplementedException("This crypto provider does not supports remove hash feature.");
            }
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.EncryptAesEcbAsync(chunk, Transformers.CreateKey(KeyStore, process), cancellationToken);

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.DecryptAesEcbAsync(cipher, Transformers.CreateKey(KeyStore, process), cancellationToken);
    }
}
