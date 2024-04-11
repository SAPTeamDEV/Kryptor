using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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

        public delegate void ProgressCallback(int progress);

        public event ProgressCallback OnProgress;

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
                    double step = (double)((double)blockSize / f.Length) * 100;
                    int counter = 1;

                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        int actualSize = (int)Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        await f.ReadAsync(slice, 0, slice.Length);
                        var eSlice = EncryptBlock(slice);
                        await f2.WriteAsync(eSlice, 0, eSlice.Length);
                        int prog = (int)Math.Round(step * counter);
                        OnProgress?.Invoke(Math.Min(prog, 100));
                        counter++;
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
                    double step = (double)((double)blockSize / f.Length) * 100;
                    int counter = 1;

                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        var actualSize = Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        await f.ReadAsync(slice, 0, slice.Length);
                        var eSlice = DecryptBlock(slice);
                        await f2.WriteAsync(eSlice, 0, eSlice.Length);
                        int prog = (int)Math.Round(step * counter);
                        OnProgress?.Invoke(Math.Min(prog, 100));
                        counter++;
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

            return bytes.RawSha256()
                                 .Concat(Encrypt(bytes.Slice<byte>(EncChunkSize)))
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
        IEnumerable<byte> Encrypt(IEnumerable<byte[]> data)
        {
            int i = 0;

            foreach (var chunk in data)
            {
                foreach (var b in AESEncryptProvider.AESEncrypt(chunk, keystore[i++]))
                {
                    yield return b;
                }
            }
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

            var decrypted = Decrypt(encrypted).ToArray();

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
        IEnumerable<byte> Decrypt(IEnumerable<byte[]> ciphers)
        {
            int i = 0;

            foreach (var cipher in ciphers)
            {
                foreach (var b in AESEncryptProvider.AESDecrypt(cipher, keystore[i++]))
                {
                    yield return b;
                }
            }
        }
    }
}
