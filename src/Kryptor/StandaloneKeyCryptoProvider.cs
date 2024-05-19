using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using EnsureThat;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides Standalone Key (SK) Crypto mechanism.
    /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
    /// </summary>
    public class StandaloneKeyCryptoProvider : CryptoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneKeyCryptoProvider"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="header">
        /// The header to initialize <see cref="StandaloneKeyCryptoProvider"/>.
        /// </param>
        public StandaloneKeyCryptoProvider(KeyStore keyStore, Header header) : this(keyStore, (bool)header.Continuous, (bool)header.RemoveHash) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneKeyCryptoProvider"/> class.
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
        public StandaloneKeyCryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false)
        {
            KeyStore = keyStore;
            Continuous = continuous;
            RemoveHash = removeHash;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, byte[] hash)
        {
            return await EncryptAsync(chunk, KeyStore[ChunkIndex]);
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<byte>> DecryptChunkAsync(byte[] cipher, byte[] hash)
        {
            return await DecryptAsync(cipher, KeyStore[ChunkIndex]);
        }

        /// <inheritdoc/>
        protected internal override void ModifyHeader(Header header)
        {
            base.ModifyHeader(header);

            if ((int)header.DetailLevel > 2)
            {
                header.CryptoType = CryptoTypes.SK;
            }
        }

        private static async Task<byte[]> EncryptAsync(byte[] data, byte[] key)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            Ensure.Enumerable.HasItems(key, nameof(key));
            Ensure.Enumerable.SizeIs(key, 32, nameof(key));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.ECB; // Use ECB mode
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

        private static async Task<byte[]> DecryptAsync(byte[] data, byte[] key)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            Ensure.Enumerable.HasItems(key, nameof(key));
            Ensure.Enumerable.SizeIs(key, 32, nameof(key));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.ECB; // Use ECB mode
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
