using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NETCore.Encrypt;
using NETCore.Encrypt.Extensions;
using NETCore.Encrypt.Shared;

namespace Kryptor
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using the Kryptor Encryption Standard (KES).
    /// </summary>
    public class KESProvider
    {
        KeyStore keystore;

        const int ChunkSize = 32;
        const int BufferChunkSize = ChunkSize - 1;
        const int BlockSize = 1048576;

        /// <summary>
        /// Max size of buffer data for encryption
        /// </summary>
        public const int BufferBlockSize = ((BlockSize / ChunkSize) - 1) * BufferChunkSize;

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

        public void EncryptFile(string path, string destination)
        {
            using (var f = File.OpenRead(path))
            {
                using (var f2 = File.OpenWrite(destination))
                {
                    int blockSize = BufferBlockSize;
                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        var actualSize = Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        f.Read(slice);
                        var eSlice = EncryptBlock(slice);
                        f2.Write(eSlice);
                    }
                }
            }
        }

        public void DecryptFile(string path, string destination)
        {
            using (var f = File.OpenRead(path))
            {
                using (var f2 = File.OpenWrite(destination))
                {
                    int blockSize = BlockSize;
                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        var actualSize = Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        f.Read(slice);
                        var eSlice = DecryptBlock(slice);
                        f2.Write(eSlice);
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts the input data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="bytes">
        /// The data to encrypt.
        /// </param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public byte[] EncryptBlock(byte[] bytes)
        {
            Check.Argument.IsNotEmpty(bytes, nameof(bytes));
            if (bytes.Length > BufferBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{BufferBlockSize}");
            }

            byte[] ciphers = Encrypt(bytes.Slice<byte>(BufferChunkSize));

            return bytes.RawSha256()
                                 .Concat(ciphers)
                                 .ToArray();

        }

        byte[] Encrypt(IEnumerable<byte[]> data)
        {
            byte[][] t = new byte[data.Count()][];
            int count = 0;
            int i = 0;
            int j = 0;

            foreach (var chunk in data)
            {
                var b = ExtraEncryptProvider.AESEncrypt(chunk, keystore[i++]);
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

        /// <summary>
        /// Decrypts the input data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="bytes">
        /// The data to decrypt.
        /// </param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the hash of the decrypted data does not match the hash in the input data.
        /// </exception>
        public byte[] DecryptBlock(byte[] bytes)
        {
            Check.Argument.IsNotEmpty(bytes, nameof(bytes));
            if (bytes.Length > BlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{BlockSize}");
            }

            var chunks = bytes.Slice<byte>(ChunkSize).ToArray();
            var hash = chunks[0];
            var encrypted = chunks[1..];

            var decrypted = Decrypt(encrypted);

            if (BitConverter.ToString(decrypted.RawSha256()) != BitConverter.ToString(hash))
            {
                throw new Exception("Hash mismatch");
            }

            return decrypted;
        }

        byte[] Decrypt(IEnumerable<byte[]> ciphers)
        {
            byte[][] t = new byte[ciphers.Count()][];
            int count = 0;
            int i = 0;
            int j = 0;

            foreach (var cipher in ciphers)
            {
                byte[] b = ExtraEncryptProvider.AESDecrypt(cipher, keystore[i++]);
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
