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
    public class KES
    {
        int index = 0;

        #region Size Parameters

        const int DecChunkSize = 32;
        const int EncChunkSize = DecChunkSize - 1;
        const int DecBlockSize = 1048576;
        const int EncBlockSize = (DecBlockSize / DecChunkSize - 1) * EncChunkSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="DecryptBlockAsync(byte[])"/>.
        /// </summary>
        public int DecryptionBlockSize { get; } = DecBlockSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="EncryptBlockAsync(byte[])"/>.
        /// </summary>
        public int EncryptionBlockSize { get; } = EncBlockSize;

        #endregion

        static readonly byte[] HeaderPattern = new byte[] { 59, 197, 2, 46, 83 };

        /// <summary>
        /// Gets or sets the keystore for crypto operations.
        /// </summary>
        public KESKeyStore KeyStore { get; set; }

        /// <summary>
        /// Gets or sets the configuration of continuous encryption method.
        /// </summary>
        public bool Continuous { get; set; }

        /// <summary>
        /// Called when a part of file is encrypted or decrypted.
        /// </summary>
        public event Action<int> OnProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="KES"/> class.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore for crypto operations.
        /// </param>
        /// <param name="continuous">
        /// Use continuous encryption method.
        /// </param>
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public KES(KESKeyStore keyStore, bool continuous = false, int maxBlockSize = default) : this(continuous, maxBlockSize)
        {
            KeyStore = keyStore;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KES"/> class.
        /// </summary>
        /// <param name="continuous">
        /// Use continuous encryption method.
        /// </param>
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public KES(bool continuous = false, int maxBlockSize = default)
        {
            if (maxBlockSize > 0)
            {
                if (!ValidateBlockSize(maxBlockSize))
                {
                    throw new ArgumentException("maxBlockSize must be a multiple of " + DecChunkSize);
                }

                DecryptionBlockSize = maxBlockSize;
                EncryptionBlockSize = (DecryptionBlockSize / DecChunkSize - 1) * EncChunkSize;
            }

            Continuous = continuous;
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

            List<byte> header = new List<byte>();
            header.AddRange(KeyStore.Fingerprint);
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

            foreach (var chunk in bytes.Chunk(EncChunkSize))
            {
                result.AddRange(await AESEncryptProvider.EncryptAsync(chunk, KeyStore[index++]));
            }

            if (!Continuous)
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

            var chunks = bytes.Chunk(DecChunkSize).ToArray();
            var hash = chunks[0];

            List<byte> result = new List<byte>();

            foreach (var cipher in chunks.Skip(1))
            {
                result.AddRange(await AESEncryptProvider.DecryptAsync(cipher, KeyStore[index++]));
            }

            if (!Continuous)
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
