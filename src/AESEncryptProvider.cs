﻿// Some parts of this file copied from NETCore.Encrypt project
// https://github.com/myloveCc/NETCore.Encrypt

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SAPTeam.Kryptor
{
    internal class AESEncryptProvider
    {
        /// <summary>
        /// Encrypts data with AES-ECB method
        /// </summary>
        /// <param name="data">Raw data</param>
        /// <param name="key">Key, requires 256 bits</param>
        /// <returns>Encrypted bytes</returns>
        public static byte[] AESEncrypt(byte[] data, string key)
        {
            Check.Argument.IsNotEmpty(data, nameof(data));
            Check.Argument.IsNotEmpty(key, nameof(key));
            Check.Argument.IsEqualLength(key.Length, 32, nameof(key));

            using (MemoryStream memory = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] bKey = new byte[32];
                    Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.KeySize = 256;
                    aes.Key = bKey;

                    using (CryptoStream cryptoStream = new CryptoStream(memory, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        try
                        {
                            cryptoStream.Write(data, 0, data.Length);
                            cryptoStream.FlushFinalBlock();
                            return memory.ToArray();
                        }
                        catch (Exception)
                        {
                            return null;
                        }
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
        public static byte[] AESDecrypt(byte[] data, string key)
        {
            Check.Argument.IsNotEmpty(data, nameof(data));
            Check.Argument.IsNotEmpty(key, nameof(key));
            Check.Argument.IsEqualLength(key.Length, 32, nameof(key));

            byte[] bKey = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

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
                        aes.Key = bKey;

                        using (CryptoStream decryptor = new CryptoStream(memory, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (MemoryStream tempMemory = new MemoryStream())
                            {
                                byte[] buffer = new byte[1024];
                                int readBytes = 0;
                                while ((readBytes = decryptor.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    tempMemory.Write(buffer, 0, readBytes);
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
                return null;
            }
        }
    }
}