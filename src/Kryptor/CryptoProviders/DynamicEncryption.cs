using SAPTeam.Kryptor.Extensions;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.CryptoProviders
{
    /// <summary>
    /// Provides Dynamic Encryption (DE) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a dynamic key and iv generated with attention of all parameters and offers the highest security.
    /// </summary>
    public sealed class DynamicEncryption : CryptoProvider
    {
        /// <summary>
        /// Creates an empty crypto provider.
        /// </summary>
        public DynamicEncryption()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEncryption"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize the crypto provider
        /// </param>
        public DynamicEncryption(KeyStore keyStore, CryptoProviderConfiguration configuration = null) => ApplyHeader(keyStore, configuration);

        /// <inheritdoc/>
        public override async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process, CancellationToken cancellationToken)
        {
            process.ProcessData[$"b{process.BlockIndex}.sha512"] = data.Sha512();
            return await base.EncryptBlockAsync(data, process, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process, CancellationToken cancellationToken)
        {
            byte[] decData = await base.DecryptBlockAsync(data, process, cancellationToken);
            process.ProcessData[$"b{process.BlockIndex}.sha512"] = decData.Sha512();
            return decData;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.EncryptAesCbcAsync(chunk, CreateDynamicKey(process), CreateMixedIV(KeyStore, process), cancellationToken);

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process, CancellationToken cancellationToken) => await AesHelper.DecryptAesCbcAsync(cipher, CreateDynamicKey(process), CreateMixedIV(KeyStore, process), cancellationToken);

        private byte[] CreateDynamicKey(CryptoProcess process)
        {
            byte[] key1 = KeyStore[Configuration.Continuous ? (process.ChunkIndex + process.BlockIndex) * 7 : (process.ChunkIndex - process.BlockIndex) * 3];
            byte[] key2 = KeyStore[process.ChunkIndex - (Configuration.RemoveHash ? key1[4] : process.BlockHash[key1[27] % 32])];
            byte[] key3 = KeyStore[process.ChunkIndex + process.BlockSize];

            byte[] mKey = process.BlockIndex > 0
                ? ((byte[])process.ProcessData[$"b{process.BlockIndex - 1}.sha512"]).Concat(key1).Concat(key2).Concat(key3).Concat(process.BlockHash).ToArray()
                : key1.Concat(key2).Concat(key3).Concat(process.BlockHash).ToArray();

            return mKey.Sha256();
        }

        internal static byte[] CreateMixedIV(KeyStore keyStore, CryptoProcess process)
        {
            int index = process.ChunkIndex + process.BlockIndex;

            return new byte[16]
            {
                keyStore[index * 6][4],
                keyStore[index * 2][12],
                keyStore[index - 154][7],
                keyStore[index + 53][19],
                keyStore[(index + 5) * 6][9],
                keyStore[index / 4][13],
                keyStore[index - 79][23],
                keyStore[index + 571][0],
                keyStore[index % 3][21],
                keyStore[index + 1][16],
                keyStore[index - 98][13],
                keyStore[index + 65][23],
                keyStore[index - 61][8],
                keyStore[index + 34][2],
                keyStore[index + 79][9],
                keyStore[index - 172][6],
            };
        }

        internal static int GetDynamicBlockSize(KeyStore keyStore, CryptoProcess process)
        {
            int index = process.BlockIndex;

            byte[] lastBytes = keyStore.Raw[(keyStore.Raw.Length - 5 - index)..^(1 + index)];
            return Math.Abs(BitConverter.ToInt32(lastBytes, 0) % 0xC000);
        }

        internal static int GetDynamicBlockEntropy(KeyStore keyStore, CryptoProcess process)
        {
            int index = process.BlockIndex * 4;

            byte[] lastBytes = keyStore.Raw[index..(index + 5)];
            return BitConverter.ToInt32(lastBytes, 0) % 0xC000;
        }

        internal static int GetDynamicChunkEntropy(KeyStore keyStore, CryptoProcess process)
        {
            int index = process.BlockIndex + process.ChunkIndex;

            byte[] lastBytes = keyStore.Raw[index..(index + 5)];
            return BitConverter.ToInt32(lastBytes, 0) % 0xC000;
        }
    }
}
