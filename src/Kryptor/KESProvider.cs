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
        readonly KESKeyStore keystore;
        readonly bool continuous = false;
        int index = 0;

        const int DecChunkSize = 32;
        const int EncChunkSize = DecChunkSize - 1;
        const int DecBlockSize = 1048576;
        const int EncBlockSize = (DecBlockSize / DecChunkSize - 1) * EncChunkSize;

        static byte[] HeaderPattern = new byte[] { 59, 197, 2, 46, 83 };

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
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        /// <param name="continuous">
        /// Use Continuous encryption method.
        /// </param>
        public KESProvider(KESKeyStore keystore, int maxBlockSize = default, bool continuous = false)
        {
            this.keystore = keystore;

            if (maxBlockSize > 0)
            {
                if (!ValidateBlockSize(maxBlockSize))
                {
                    throw new ArgumentException("maxBlockSize must be a multiple of " + DecChunkSize);
                }

                DecryptionBlockSize = maxBlockSize;
                EncryptionBlockSize = (DecryptionBlockSize / DecChunkSize - 1) * EncChunkSize;
            }

            this.continuous = continuous;
        }

        /// <summary>
        /// Validates the divisibility of the given block size.
        /// </summary>
        /// <param name="bs">
        /// The block size.
        /// </param>
        /// <returns></returns>
        public static bool ValidateBlockSize(int bs)
        {
            if (bs % DecChunkSize != 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the header of a kef file.
        /// </summary>
        /// <param name="stream">
        /// File stream of source file with read ability.
        /// </param>
        public static (int end, byte[] fingerprint, string fileName) ReadHeader(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            // Get first 512B to search for header.
            byte[] buffer = new byte[Math.Min(512, stream.Length)];
            stream.Read(buffer, 0, buffer.Length);

            int loc = buffer.LocatePattern(HeaderPattern);
            var data = buffer.Take(loc).ToArray();

            byte[] signature = data.Take(16).ToArray();
            byte[] name = data.Skip(16).ToArray();

            stream.Seek(0, SeekOrigin.Begin);

            return (loc + HeaderPattern.Length, signature, Encoding.UTF8.GetString(name));
        }

        /// <summary>
        /// Encrypts the given data and writes the encrypted data to the specified file stream.
        /// </summary>
        /// <param name="source">
        /// File stream of source filr with read ability.
        /// </param>
        /// <param name="dest">
        /// File stream of destination file with write ability.
        /// </param>
        public async Task EncryptFileAsync(FileStream source, FileStream dest)
        {
            index = 0;

            List<byte>header = new List<byte>();
            header.AddRange(keystore.Fingerprint);
            header.AddRange(Encoding.UTF8.GetBytes(Path.GetFileName(source.Name)));
            header.AddRange(HeaderPattern);
            byte[] hArray = header.ToArray();

            await dest.WriteAsync(hArray, 0, hArray.Length);

            int blockSize = EncryptionBlockSize;
            double step = (double)((double)blockSize / source.Length) * 100;
            int counter = 1;
            int lastProg = -1;
            OnProgress?.Invoke(0);

            for (long i = 0; i < source.Length; i += blockSize)
            {
                int actualSize = (int)Math.Min(source.Length - i, blockSize);
                byte[] slice = new byte[actualSize];
                await source.ReadAsync(slice, 0, slice.Length);
                var eSlice = await EncryptBlockAsync(slice);
                await dest.WriteAsync(eSlice, 0, eSlice.Length);
                int prog = (int)Math.Round(step * counter);
                if (prog != lastProg)
                {
                    OnProgress?.Invoke(Math.Min(prog, 100));
                    lastProg = prog;
                }
                counter++;
            }

            index = 0;
        }

        /// <summary>
        /// Decrypts the given data and writes the decrypted data to the specified file stream.
        /// </summary>
        /// <param name="source">
        /// File stream of source filr with read ability.
        /// </param>
        /// <param name="dest">
        /// File stream of destination file with write ability.
        /// </param>
        public async Task DecryptFileAsync(FileStream source, FileStream dest)
        {
            index = 0;

            source.Seek(ReadHeader(source).end, SeekOrigin.Begin);

            int blockSize = DecryptionBlockSize;
            double step = (double)((double)blockSize / source.Length) * 100;
            int counter = 1;
            int lastProg = -1;
            OnProgress?.Invoke(0);

            for (long i = 0; i < source.Length; i += blockSize)
            {
                var actualSize = Math.Min(source.Length - source.Position, blockSize);
                byte[] slice = new byte[actualSize];
                await source.ReadAsync(slice, 0, slice.Length);
                var eSlice = await DecryptBlockAsync(slice);
                await dest.WriteAsync(eSlice, 0, eSlice.Length);
                int prog = (int)Math.Round(step * counter);
                if (prog != lastProg)
                {
                    OnProgress?.Invoke(Math.Min(prog, 100));
                    lastProg = prog;
                }
                counter++;
            }

            index = 0;
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

            List<byte> result = new List<byte>(bytes.Sha256());

            foreach (var chunk in bytes.Slice<byte>(EncChunkSize))
            {
                result.AddRange(await AESEncryptProvider.EncryptAsync(chunk, keystore[index++]));
            }

            if (!continuous)
            {
                index = 0;
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

            List<byte> result = new List<byte>();

            foreach (var cipher in chunks.Skip(1))
            {
                result.AddRange(await AESEncryptProvider.DecryptAsync(cipher, keystore[index++]));
            }

            if (!continuous)
            {
                index = 0;
            }

            var array = result.ToArray();

            if (!hash.SequenceEqual(array.Sha256()))
            {
                throw new InvalidDataException("Hash mismatch");
            }

            return array;
        }
    }
}
