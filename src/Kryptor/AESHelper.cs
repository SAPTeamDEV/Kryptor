// Some parts of this file copied from NETCore.Encrypt project
// https://github.com/myloveCc/NETCore.Encrypt

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using EnsureThat;

namespace SAPTeam.Kryptor
{
    internal class AESHelper
    {
        /// <summary>
        /// Encrypts data with AES-ECB method
        /// </summary>
        /// <param name="data">Raw data</param>
        /// <param name="key">Key, requires 256 bits</param>
        /// <returns>Encrypted bytes</returns>
        public static async Task<byte[]> EncryptAsync(byte[] data, byte[] key)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            Ensure.Enumerable.HasItems(key, nameof(key));
            Ensure.Enumerable.SizeIs(key, 32, nameof(key));

            using (MemoryStream memory = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.KeySize = 256;
                    aes.Key = key;

                    using (CryptoStream cryptoStream = new CryptoStream(memory, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return memory.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts data with AES-ECB method
        /// </summary>
        /// <param name="data">Encrypted data</param>
        /// <param name="key">Key, requires 256 bits</param>
        /// <returns>Decrypted bytes</returns>
        public static async Task<byte[]> DecryptAsync(byte[] data, byte[] key)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            Ensure.Enumerable.HasItems(key, nameof(key));
            Ensure.Enumerable.SizeIs(key, 32, nameof(key));

            try
            {
                byte[] decryptedData = null; // decrypted data

                using (MemoryStream memory = new MemoryStream(data))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Mode = CipherMode.ECB;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.KeySize = 256;
                        aes.Key = key;

                        using (CryptoStream decryptor = new CryptoStream(memory, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (MemoryStream tempMemory = new MemoryStream())
                            {
                                byte[] buffer = new byte[1024];
                                int readBytes = 0;
                                while ((readBytes = decryptor.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    await tempMemory.WriteAsync(buffer, 0, readBytes);
                                }

                                decryptedData = tempMemory.ToArray();
                                return decryptedData;
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new InvalidDataException("Key is invalid");
            }
        }
    }
}
