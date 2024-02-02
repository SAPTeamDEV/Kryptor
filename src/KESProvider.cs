using System;
using System.Collections.Generic;
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
            var chunks = data.Divide(256);
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
    }
}
