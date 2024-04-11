using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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

        static byte[] HeaderPattern = new byte[] { 59, 197, 2, 46, 83 };
        static int HeaderSize = 280;

        /// <summary>
        /// Delegate for OnProgress event.
        /// </summary>
        /// <param name="progress">
        /// The progress of the operation.
        /// </param>
        public delegate void ProgressCallback(int progress);

        /// <summary>
        /// Called when a part of file is encrypted or decrypted.
        /// </summary>
        public event ProgressCallback OnProgress;

        /// <summary>
        /// Gets max input buffer size for <see cref="DecryptBlockAsync(byte[])"/>.
        /// </summary>
        public int DecryptionBlockSize { get; } = DecBlockSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="EncryptBlockAsync(byte[])"/>.
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

        public static (byte[] signature, string fileName) ReadHeader(Stream f)
        {
            byte[] buffer = new byte[HeaderSize];
            f.Read(buffer, 0, buffer.Length);

            int loc = buffer.LocatePattern(HeaderPattern);
            var data = buffer.Take(loc).ToArray();

            byte[] signature = data.Take(16).ToArray();
            byte[] name = data.Skip(16).ToArray();

            return (signature, Encoding.UTF8.GetString(name));
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
                    byte[] header = new byte[HeaderSize];
                    keystore.Fingerprint.CopyTo(header, 0);
                    byte[] nameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(path));
                    nameBytes.CopyTo(header, 16);
                    HeaderPattern.CopyTo(header, 16 + nameBytes.Length);
                    await f2.WriteAsync(header, 0, header.Length);

                    int blockSize = EncryptionBlockSize;
                    double step = (double)((double)blockSize / f.Length) * 100;
                    int counter = 1;

                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        int actualSize = (int)Math.Min(f.Length - i, blockSize);
                        byte[] slice = new byte[actualSize];
                        await f.ReadAsync(slice, 0, slice.Length);
                        var eSlice = await EncryptBlockAsync(slice);
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
        public async Task DecryptFileAsync(string path)
        {
            using (var f = File.OpenRead(path))
            {
                string origName = ReadHeader(f).fileName;
                string destination = Path.Combine(Directory.GetParent(path).FullName, origName);
                int suffix = 2;

                while (File.Exists(destination))
                {
                    string tempName = $"{Path.GetFileNameWithoutExtension(destination)} ({suffix++}).{Path.GetExtension(destination)}";
                    if (!File.Exists(Path.Combine(Directory.GetParent(path).FullName, tempName)))
                    {
                        destination = Path.Combine(Directory.GetParent(path).FullName, tempName);
                    }
                }

                using (var f2 = File.OpenWrite(destination))
                {
                    int blockSize = DecryptionBlockSize;
                    double step = (double)((double)blockSize / f.Length) * 100;
                    int counter = 1;

                    for (long i = 0; i < f.Length; i += blockSize)
                    {
                        var actualSize = Math.Min(f.Length - f.Position, blockSize);
                        byte[] slice = new byte[actualSize];
                        await f.ReadAsync(slice, 0, slice.Length);
                        var eSlice = await DecryptBlockAsync(slice);
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
        public async Task<byte[]> EncryptBlockAsync(byte[] bytes)
        {
            Check.Argument.IsNotEmpty(bytes, nameof(bytes));
            if (bytes.Length > EncryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{EncryptionBlockSize}");
            }

            return bytes.RawSha256()
                                 .Concat(await EncryptAsync(bytes.Slice<byte>(EncChunkSize)))
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
        async Task<byte[]> EncryptAsync(IEnumerable<byte[]> data)
        {
            List<byte> result = new List<byte>();
            int i = 0;

            foreach (var chunk in data)
            {
                foreach (var b in await AESEncryptProvider.EncryptAsync(chunk, keystore[i++]))
                {
                    result.Add(b);
                }
            }

            return result.ToArray();
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
        public async Task<byte[]> DecryptBlockAsync(byte[] bytes)
        {
            Check.Argument.IsNotEmpty(bytes, nameof(bytes));
            if (bytes.Length > DecryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{DecryptionBlockSize}");
            }

            var chunks = bytes.Slice<byte>(DecChunkSize).ToArray();
            var hash = chunks[0];
            var encrypted = chunks.Skip(1);

            var decrypted = await DecryptAsync(encrypted);

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
        async Task<byte[]> DecryptAsync(IEnumerable<byte[]> ciphers)
        {
            List<byte> result = new List<byte>();
            int i = 0;

            foreach (var cipher in ciphers)
            {
                foreach (var b in await AESEncryptProvider.DecryptAsync(cipher, keystore[i++]))
                {
                    result.Add(b);
                }
            }

            return result.ToArray();
        }
    }
}
