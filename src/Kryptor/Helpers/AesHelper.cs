using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Helpers
{
    /// <summary>
    /// Provides static methods to encrypt and decrypt data with AES algorithm.
    /// </summary>
    public static class AesHelper
    {
        internal static void CheckArgument(byte[] arg, string paramName, int size = 0)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(paramName);
            }
            else if (arg.Length == 0)
            {
                throw new ArgumentException($"{paramName} argument is empty");
            }
            else if (size > 0 && arg.Length != size)
            {
                throw new ArgumentException($"The expected size for {paramName} is {size}");
            }
        }

        /// <summary>
        /// Encrypts data with AES-ECB algorithm.
        /// </summary>
        /// <param name="data">
        /// The data to encrypt.
        /// </param>
        /// <param name="key">
        /// The 256 bit secret key.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns></returns>
        public static async Task<byte[]> EncryptAesEcbAsync(byte[] data, byte[] key, CancellationToken cancellationToken)
        {
            CheckArgument(data, nameof(data));
            CheckArgument(key, nameof(key), 32);

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
                        await csEncrypt.WriteAsync(data, 0, data.Length, cancellationToken);
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts data with AES-ECB algorithm.
        /// </summary>
        /// <param name="data">
        /// The data to decrypt.
        /// </param>
        /// <param name="key">
        /// The 256 bit secret key.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns></returns>
        public static async Task<byte[]> DecryptAesEcbAsync(byte[] data, byte[] key, CancellationToken cancellationToken)
        {
            CheckArgument(data, nameof(data));
            CheckArgument(key, nameof(key), 32);

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
                                    await tempMemory.WriteAsync(buffer, 0, readBytes, cancellationToken);
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

        /// <summary>
        /// Encrypts data with AES-CBC algorithm.
        /// </summary>
        /// <param name="data">
        /// The data to encrypt.
        /// </param>
        /// <param name="key">
        /// The 256 bit secret key.
        /// </param>
        /// <param name="iv">
        /// the 128 bit initialization vector.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns></returns>
        public static async Task<byte[]> EncryptAesCbcAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken)
        {
            CheckArgument(key, nameof(key), 32);
            CheckArgument(iv, nameof(iv), 16);

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
                        await csEncrypt.WriteAsync(data, 0, data.Length, cancellationToken);
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts data with AES-CBC algorithm.
        /// </summary>
        /// <param name="data">
        /// The data to decrypt.
        /// </param>
        /// <param name="key">
        /// The 256 bit secret key.
        /// </param>
        /// <param name="iv">
        /// the 128 bit initialization vector.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns></returns>
        public static async Task<byte[]> DecryptAesCbcAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken)
        {
            CheckArgument(key, nameof(key), 32);
            CheckArgument(iv, nameof(iv), 16);

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
                                    await tempMemory.WriteAsync(buffer, 0, readBytes, cancellationToken);
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
