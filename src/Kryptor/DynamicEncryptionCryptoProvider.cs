﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides Dynamic Encryption (DE) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a dynamic key and iv generated with attention of all parameters and offers the highest security.
    /// </summary>
    public sealed class DynamicEncryptionCryptoProvider : CryptoProvider
    {
        /// <inheritdoc/>
        public override string Name => "DynamicEncryption";

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEncryptionCryptoProvider"/> class.
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
        public DynamicEncryptionCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false) : base(keyStore, continuous, removeHash)
        {

        }

        /// <inheritdoc/>
        public override async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process)
        {
            process.ProcessData[$"b{process.BlockIndex}.sha512"] = data.Sha512();
            return await base.EncryptBlockAsync(data, process);
        }

        /// <inheritdoc/>
        public override async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process)
        {
            var decData = await base.DecryptBlockAsync(data, process);
            process.ProcessData[$"b{process.BlockIndex}.sha512"] = decData.Sha512();
            return decData;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await AesHelper.EncryptAesCbcAsync(chunk, CreateDynamicKey(process), CreateMixedIV(KeyStore, process));
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await AesHelper.DecryptAesCbcAsync(cipher, CreateDynamicKey(process), CreateMixedIV(KeyStore, process));
        }

        /// <inheritdoc/>
        protected internal override void UpdateHeader(Header header)
        {
            base.UpdateHeader(header);

            if ((int)header.DetailLevel > 2)
            {
                header.CryptoType = CryptoTypes.DE;
            }
        }

        private byte[] CreateDynamicKey(CryptoProcess process)
        {
            var key1 = KeyStore[Continuous ? (process.ChunkIndex + process.BlockIndex) * 7 : (process.ChunkIndex - process.BlockIndex) * 3];
            var key2 = KeyStore[process.ChunkIndex - (RemoveHash ? key1[4] : process.BlockHash[key1[27] % 32])];
            var key3 = KeyStore[process.ChunkIndex + Parent.BlockSize];

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
    }
}
