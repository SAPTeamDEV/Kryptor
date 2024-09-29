using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Transformed Key (TK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a mix-transformed key.
    /// </summary>
    public sealed class TransformedKey : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "Transformed Key";

        /// <inheritdoc/>
        public override bool IsSecure => false;

        /// <summary>
        /// Creates an empty crypto provider.
        /// </summary>
        public TransformedKey()
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
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.EncryptAesEcbAsync(chunk, Transformers.CreateKey(KeyStore, process), cancellationToken);

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.DecryptAesEcbAsync(cipher, Transformers.CreateKey(KeyStore, process), cancellationToken);
    }
}
