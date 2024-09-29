using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Transformed Parameters (TP) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key and iv.
    /// </summary>
    public sealed class TransformedParameters : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "Transformed Parameters";

        /// <summary>
        /// Creates an empty crypto provider.
        /// </summary>
        public TransformedParameters()
        {

        }

        /// <inheritdoc/>
        protected override void Initialize(KeyStore keyStore, CryptoProviderConfiguration configuration = null)
        {
            base.Initialize(keyStore, configuration);

            if (Configuration.RemoveHash)
            {
                throw new NotImplementedException("This crypto provider does not supports remove hash feature.");
            }
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.EncryptAesCbcAsync(chunk, Transformers.CreateKey(KeyStore, process), Transformers.CreateIV(KeyStore, process), cancellationToken);

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.DecryptAesCbcAsync(cipher, Transformers.CreateKey(KeyStore, process), Transformers.CreateIV(KeyStore, process), cancellationToken);
    }
}
