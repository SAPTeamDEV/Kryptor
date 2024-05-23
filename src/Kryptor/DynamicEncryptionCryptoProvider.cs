using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    public class DynamicEncryptionCryptoProvider : CryptoProvider
    {
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
        public DynamicEncryptionCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false)
        {
            KeyStore = keyStore;
            Continuous = continuous;
            RemoveHash = removeHash;
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
                header.CryptoType = CryptoTypes.DE;
            }
        }

        private byte[] CreateKey(CryptoProcess process)
        {
            byte[] mKey;

            var key1 = KeyStore[Continuous ? (process.ChunkIndex + process.BlockIndex) * 7 : (process.ChunkIndex - process.BlockIndex) * 3];
            var key2 = KeyStore[process.ChunkIndex - (RemoveHash ? key1[4] : process.BlockHash[key1[27] % 32])];
            var key3 = KeyStore[process.ChunkIndex + Parent.BlockSize];


            /* Unmerged change from project 'Kryptor (net461)'
            Before:
                        if (process.BlockIndex > 0)
            After:
                        mKey = process.BlockIndex > 0)
            */
            mKey = process.BlockIndex > 0

                /* Unmerged change from project 'Kryptor (net461)'
                Before:
                                mKey = Transformers.Mix(((key1[30] + key3[2]) * key3[18]) - key2[5], (byte[])process.ProcessData[$"b{process.BlockIndex - 1}.sha512"], key1, key2, key3, process.BlockHash).ToArray();
                            }
                            else
                            {
                                mKey = Transformers.Mix((key1[8] + key1[13] + key2[28]) * key3[11], key1, key2, key3, process.BlockHash).ToArray();
                After:
                                mKey = Transformers.Mix(((key1[30] + key3[2]) * key3[18]) - key2[5], (byte[])process.ProcessData[$"b{process.BlockIndex - 1}.sha512"], key1, key2, key3, process.BlockHash).ToArray()
                                mKey = Transformers.Mix((key1[8] + key1[13] + key2[28]) * key3[11], key1, key2, key3, process.BlockHash).ToArray();
                */
                ? Transformers.Mix(((key1[30] + key3[2]) * key3[18]) - key2[5], (byte[])process.ProcessData[$"b{process.BlockIndex - 1}.sha512"], key1, key2, key3, process.BlockHash).ToArray()
                : Transformers.Mix((key1[8] + key1[13] + key2[28]) * key3[11], key1, key2, key3, process.BlockHash).ToArray();

            return mKey.Sha256();
        }

        private byte[] CreateIV(CryptoProcess process)
        {
            int index = process.ChunkIndex + process.BlockIndex;

            return new byte[16]
            {
                KeyStore[index * 6][4],
                KeyStore[index * 2][12],
                KeyStore[index - 154][7],
                KeyStore[index + 53][19],
                KeyStore[(index + 5) * 6][9],
                KeyStore[index / 4][13],
                KeyStore[index - 79][23],
                KeyStore[index + 571][0],
                KeyStore[index % 3][21],
                KeyStore[index + 1][16],
                KeyStore[index - 98][13],
                KeyStore[index + 65][23],
                KeyStore[index - 61][8],
                KeyStore[index + 34][2],
                KeyStore[index + 79][9],
                KeyStore[index - 172][6],
            };
        }
    }
}
