using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETCore.Encrypt;
using NETCore.Encrypt.Extensions;

namespace Kryptor
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using the Kryptor Encryption Standard (KES).
    /// </summary>
    public class KESProvider
    {
        KeyStore keystore;
        const int ChUNK_SIZE = 863;
        const char CHUNK_DELIMITER = ';';

        /// <summary>
        /// Initializes a new instance of the <see cref="KESProvider"/> class.
        /// </summary>
        /// <param name="keystore">
        /// The keystore to use for encryption and decryption.
        /// </param>
        public KESProvider(KeyStore keystore)
        {
            this.keystore = keystore;
        }

        /// <summary>
        /// Encrypts the specified data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="data">
        /// The data to encrypt.
        /// </param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public string Encrypt(string data)
        {
            var _chunks = data.ToCharArray().Slice<char>(256);
            string[] chunks = new string[Math.Max(data.Length, 256) / 256];

            int i = 0;

            foreach (var chunk in _chunks)
            {
                chunks[i++] = new string(chunk).Trim('\0');
            }

            var ciphers = Encrypt(chunks);

            string cipher = data.SHA256();
            cipher += string.Join("\\", ciphers);

            return EncryptProvider.Base64Encrypt(cipher);
        }

        IEnumerable<string> Encrypt(IEnumerable<string> data)
        {
            int i = 0;

            foreach (var chunk in data)
            {
                yield return EncryptProvider.AESEncrypt(EncryptProvider.Base64Encrypt(chunk), keystore[i++]);
            }
        }

        /// <summary>
        /// Decrypts the specified data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="data">
        /// The data to decrypt.
        /// </param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the hash of the decrypted data does not match the hash in the input data.
        /// </exception>
        public string Decrypt(string data)
        {
            var encrypted = EncryptProvider.Base64Decrypt(data);
            var ciphers = encrypted[64..].Split('\\');
            var hash = encrypted[..64];

            var chunks = Decrypt(ciphers);
            var decrypted = string.Join("", chunks);

            if (decrypted.SHA256() != hash)
            {
                throw new Exception("Hash mismatch");
            }

            return decrypted;
        }

        IEnumerable<string> Decrypt(string[] ciphers)
        {
            int i = 0;

            foreach (var cipher in ciphers)
            {
                yield return EncryptProvider.Base64Decrypt(EncryptProvider.AESDecrypt(cipher, keystore[i++]));
            }
        }

        /// <summary>
        /// Encrypts the specified data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="data">
        /// The data to encrypt.
        /// </param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public string Encrypt(byte[] data, int size=ChUNK_SIZE)
        {
            var chunks = data.Slice<byte>(size);

            var ciphers = tEncrypt(chunks);
            var cipher = data.Sha256();
            cipher += string.Join(CHUNK_DELIMITER, ciphers);

            /*
            Console.WriteLine("Encryption Stage:");
            Console.WriteLine($"{data.Length} bytes");
            Console.WriteLine($"Number of Chuncks: {chunks.Count()}, last Cunck Size: {chunks.Last().Length}");
            Console.WriteLine($"Total Segments: {ciphers.Count()}");
            Console.WriteLine($"Raw Output Size: {cipher.Length}");
            */

            // return EncryptProvider.Base64Encrypt(cipher);
            return cipher;
        }

        IEnumerable<string> tEncrypt(IEnumerable<byte[]> data)
        {
            int i = 0;

            foreach (var chunk in data)
            {
                var b = ExtraEncryptProvider.AESEncrypt(chunk, keystore[i++]);
                var b64 = Convert.ToBase64String(b);
                // Console.WriteLine($"Index: {i}, Chunck Size: {chunk.Length}, Encrypted Size: {b.Length}, b64 Length: {b64.Length}");
                yield return b64;
            }
        }

        /// <summary>
        /// Decrypts the specified data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="data">
        /// The data to decrypt.
        /// </param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the hash of the decrypted data does not match the hash in the input data.
        /// </exception>
        public byte[] DecryptByte(string data)
        {
            var ciphers = data[64..].Split(CHUNK_DELIMITER);
            var hash = data[..64];

            var decrypted = DecryptByte(ciphers);

            if (decrypted.Sha256() != hash)
            {
                throw new Exception("Hash mismatch");
            }

            return decrypted;
        }

        byte[] DecryptByte(IEnumerable<string> ciphers)
        {
            byte[][] t = new byte[ciphers.Count()][];
            int count = 0;
            int i = 0;
            int j = 0;

            foreach (var cipher in ciphers)
            {
                byte[] b = ExtraEncryptProvider.AESDecrypt(Convert.FromBase64String(cipher), keystore[i++]);
                // b = b.CheckPads();
                count += b.Length;
                t[j++] = b;
            }

            byte[] buffer = new byte[count];
            int ii = 0;

            foreach (var bt in t)
            {
                bt.CopyTo(buffer, ii);
                ii += bt.Length;
            }

            return buffer;
        }
    }
}
