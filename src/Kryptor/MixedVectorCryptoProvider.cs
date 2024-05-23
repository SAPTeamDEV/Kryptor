using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using EnsureThat;

namespace SAPTeam.Kryptor
{
    public class MixedVectorCryptoProvider : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MixedVectorCryptoProvider"/> class.
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
        public MixedVectorCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false)
        {
            KeyStore = keyStore;
            Continuous = continuous;
            RemoveHash = removeHash;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process)
        {
            return await EncryptAsync(chunk, KeyStore[process.ChunkIndex], Transformers.Pick(process.BlockHash, 16, process.BlockIndex + process.ChunkIndex).ToArray());
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, CryptoProcess process)
        {
            return await DecryptAsync(cipher, KeyStore[process.ChunkIndex], Transformers.Pick(process.BlockHash, 16, process.BlockIndex + process.ChunkIndex).ToArray());
        }

        /// <inheritdoc/>
        protected internal override void UpdateHeader(Header header)
        {
            base.UpdateHeader(header);

            if ((int)header.DetailLevel > 2)
            {
                header.CryptoType = CryptoTypes.MV;
            }
        }

        internal static async Task<byte[]> EncryptAsync(byte[] data, byte[] key, byte[] iv)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            Ensure.Enumerable.HasItems(key, nameof(key));
            Ensure.Enumerable.SizeIs(key, 32, nameof(key));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC; // Use CBC mode
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        await csEncrypt.WriteAsync(data, 0, data.Length);
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        internal static async Task<byte[]> DecryptAsync(byte[] data, byte[] key, byte[] iv)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            Ensure.Enumerable.HasItems(key, nameof(key));
            Ensure.Enumerable.SizeIs(key, 32, nameof(key));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC; // Use CBC mode
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(data))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream tempMemory = new MemoryStream())
                        {
                            try
                            {
                                byte[] buffer = new byte[1024];
                                int readBytes = 0;
                                while ((readBytes = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    await tempMemory.WriteAsync(buffer, 0, readBytes);
                                }

                                return tempMemory.ToArray();
                            }
                            catch (CryptographicException)
                            {
                                throw new InvalidDataException("Cannot decrypt data.");
                            }
                        }
                    }
                }
            }
        }
    }
}
