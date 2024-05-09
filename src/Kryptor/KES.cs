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
        private ICryptoProvider provider;

        #region Size Parameters

        /// <summary>
        /// Gets the Default Decryption Chunk Size. (Only For Advanced Usages)
        /// </summary>
        public const int DefaultDecryptionChunkSize = 32;

        /// <summary>
        /// Gets the Default Encryption Chunk Size. (Only For Advanced Usages)
        /// </summary>
        public const int DefaultEncryptionChunkSize = DefaultDecryptionChunkSize - 1;

        /// <summary>
        /// Gets the Default Decryption Block Size. (Only For Advanced Usages)
        /// </summary>
        public const int DefaultDecryptionBlockSize = 1048576;

        /// <summary>
        /// Gets the Default Encryption Block Size. (Only For Advanced Usages)
        /// </summary>
        public const int DefaultEncryptionBlockSize = (DefaultDecryptionBlockSize / DefaultDecryptionChunkSize - 1) * DefaultEncryptionChunkSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.DecryptBlockAsync(byte[])"/>.
        /// </summary>
        public int DecryptionBlockSize { get; } = DefaultDecryptionBlockSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.EncryptBlockAsync(byte[])"/>.
        /// </summary>
        public int EncryptionBlockSize { get; } = DefaultEncryptionBlockSize;

        #endregion

        static readonly byte[] HeaderPattern = new byte[] { 59, 197, 2, 46, 83 };

        /// <summary>
        /// Gets or sets the crypto provider.
        /// </summary>
        public ICryptoProvider Provider
        {
            get => provider;
            set
            {
                if (value is CryptoProvider cp && cp.Parent != this)
                {
                    cp.Parent = this;
                }

                provider = value;
            }
        }

        /// <summary>
        /// Called when a part of file is encrypted or decrypted.
        /// </summary>
        public event Action<int> OnProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="KES"/> class.
        /// </summary>
        /// <param name="provider">
        /// The crypto provider.
        /// </param>
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public KES(ICryptoProvider provider, int maxBlockSize = default) : this(maxBlockSize)
        {
            Provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KES"/> class.
        /// </summary>
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public KES(int maxBlockSize = default)
        {
            if (maxBlockSize > 0)
            {
                if (!ValidateBlockSize(maxBlockSize))
                {
                    throw new ArgumentException("maxBlockSize must be a multiple of " + DefaultDecryptionChunkSize);
                }

                DecryptionBlockSize = maxBlockSize;
                EncryptionBlockSize = (DecryptionBlockSize / DefaultDecryptionChunkSize - 1) * DefaultEncryptionChunkSize;
            }
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
            if (bs % DefaultDecryptionChunkSize != 0)
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
            header.AddRange(Provider.KeyStore.Fingerprint);
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
                var eSlice = await Provider.EncryptBlockAsync(slice);
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
                var eSlice = await Provider.DecryptBlockAsync(slice);
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
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <returns>Encrypted data block.</returns>
        public async Task<byte[]> EncryptBlockAsync(byte[] data)
        {
            return await Provider.EncryptBlockAsync(data);
        }

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data)
        {
            return await Provider.DecryptBlockAsync(data);
        }
    }
}
