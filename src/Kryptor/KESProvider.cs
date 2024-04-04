using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using the Kryptor Encryption Standard (KES).
    /// </summary>
    public class KESProvider
    {
        KESKeyStore keystore;

        const int DecChunkSize = 32;
        const int EncChunkSize = DecChunkSize - 1;
        const int DecBlockSize = 1048576;
        const int EncBlockSize = (DecBlockSize / DecChunkSize - 1) * EncChunkSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="DecryptBlock(byte[])"/>.
        /// </summary>
        public int DecryptionBlockSize { get; } = DecBlockSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="EncryptBlock(byte[])"/>.
        /// </summary>
        public int EncryptionBlockSize { get; } = EncBlockSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="KESProvider"/> class.
        /// </summary>
        /// <param name="keystore">
        /// The keystore to use for encryption and decryption.
        /// </param>
        /// <param name="MaxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public KESProvider(KESKeyStore keystore, int MaxBlockSize = default)
        {
            this.keystore = keystore;

            if (MaxBlockSize > 0)
            {
                if (MaxBlockSize % DecChunkSize != 0)
                {
                    throw new ArgumentException("maxBlockSize must be a multiple of " + DecChunkSize);
                }

                DecryptionBlockSize = MaxBlockSize;
            }
        }

        /// <summary>
        /// Encrypts the given file and writes the encrypted data to the specified destination.
        /// </summary>
        /// <param name="path">
        /// The path of the file to encrypt.
        /// </param>
        /// <param name="destination">
        /// The path of the file to write the encrypted data to.
        /// </param>
        public async Task EncryptFileAsync(string path, string destination)
        {
            using (var f = File.OpenRead(path))
            {
                using (var f2 = File.OpenWrite(destination))
                {
                    int blockSize = EncryptionBlockSize;
                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        int actualSize = (int)Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        await f.ReadAsync(slice, 0, slice.Length);
                        var eSlice = EncryptBlock(slice);
                        await f2.WriteAsync(eSlice, 0, eSlice.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts the given file and writes the decrypted data to the specified destination.
        /// </summary>
        /// <param name="path">
        /// The path of the file to decrypt.
        /// </param>
        /// <param name="destination">
        /// The path of the file to write the decrypted data to.
        /// </param>
        public async Task DecryptFileAsync(string path, string destination)
        {
            using (var f = File.OpenRead(path))
            {
                using (var f2 = File.OpenWrite(destination))
                {
                    int blockSize = DecryptionBlockSize;
                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        var actualSize = Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        await f.ReadAsync(slice, 0, slice.Length);
                        var eSlice = DecryptBlock(slice);
                        await f2.WriteAsync(eSlice, 0, eSlice.Length);
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
            if (bytes.Length > EncryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{EncryptionBlockSize}");
            }

            byte[] ciphers = Encrypt(bytes.Slice<byte>(EncChunkSize));

            return bytes.RawSha256()
                                 .Concat(ciphers)
                                 .ToArray();

        }

        /// <summary>
        /// Encrypts the input data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="data">
        /// The data to encrypt.
        /// </param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        byte[] Encrypt(IEnumerable<byte[]> data)
        {
            int cc = 0;
            foreach (var chunk in data)
            {
                cc += ((chunk.Length / 16) + 1) * 16;
            }

            byte[] buffer = new byte[cc];
            int index = 0;
            int i = 0;

            foreach (var chunk in data)
            {
                var b = AESEncryptProvider.AESEncrypt(chunk, keystore[i++]);
                b.CopyTo(buffer, index);
                index += b.Length;
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
            if (bytes.Length > DecryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{DecryptionBlockSize}");
            }

            var chunks = bytes.Slice<byte>(DecChunkSize).ToArray();
            var hash = chunks[0];
            var encrypted = chunks.Skip(1);

            var decrypted = Decrypt(encrypted);

            if (BitConverter.ToString(decrypted.RawSha256()) != BitConverter.ToString(hash))
            {
                throw new InvalidDataException("Hash mismatch");
            }

            return decrypted;
        }

        /// <summary>
        /// Decrypts the input data using the Kryptor Encryption Standard (KES).
        /// </summary>
        /// <param name="ciphers">
        /// The data to decrypt.
        /// </param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        byte[] Decrypt(IEnumerable<byte[]> ciphers)
        {
            byte[][] t = new byte[ciphers.Count()][];
            int count = 0;
            int i = 0;
            int j = 0;

            foreach (var cipher in ciphers)
            {
                byte[] b = AESEncryptProvider.AESDecrypt(cipher, keystore[i++]);
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
