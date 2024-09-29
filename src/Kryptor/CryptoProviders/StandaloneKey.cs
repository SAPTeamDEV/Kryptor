using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Standalone Key (SK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
    /// </summary>
    public sealed class StandaloneKey : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "Standalone Key";

        /// <inheritdoc/>
        public override bool IsSecure => false;

        /// <summary>
        /// Creates an empty crypto provider.
        /// </summary>
        public StandaloneKey()
        {

        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.EncryptAesEcbAsync(chunk, KeyStore[process.ChunkIndex], cancellationToken);

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.DecryptAesEcbAsync(cipher, KeyStore[process.ChunkIndex], cancellationToken);
    }
}
