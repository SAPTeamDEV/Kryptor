using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using the Kryptor Encryption Standard (KES).
    /// </summary>
    public class Kes
    {
        private CryptoProvider provider;

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
        public const int DefaultEncryptionBlockSize = ((DefaultDecryptionBlockSize / DefaultDecryptionChunkSize) - 1) * DefaultEncryptionChunkSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.DecryptBlockAsync(byte[])"/>.
        /// </summary>
        public int DecryptionBlockSize { get; } = DefaultDecryptionBlockSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.EncryptBlockAsync(byte[])"/>.
        /// </summary>
        public int EncryptionBlockSize { get; } = DefaultEncryptionBlockSize;

        #endregion

        /// <summary>
        /// Gets the version of the encryptor api backend.
        /// </summary>
        public static Version Version => new Version(0, 8);

        /// <summary>
        /// Gets or sets the crypto provider.
        /// </summary>
        public CryptoProvider Provider
        {
            get => provider;
            set
            {
                if (value.Parent != this)
                {
                    value.Parent = this;
                }

                provider = value;
            }
        }

        /// <summary>
        /// Called when a part of file is encrypted or decrypted.
        /// </summary>
        public event Action<int> OnProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="provider">
        /// The crypto provider.
        /// </param>
        /// <param name="header">
        /// The header to initialize <see cref="Kes"/>.
        /// </param>
        public Kes(CryptoProvider provider, Header header) : this(header)
        {
            Provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="header">
        /// The header to initialize <see cref="Kes"/>.
        /// </param>
        public Kes(Header header) : this((int)header.BlockSize) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="provider">
        /// The crypto provider.
        /// </param>
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public Kes(CryptoProvider provider, int maxBlockSize = default) : this(maxBlockSize)
        {
            Provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="maxBlockSize">
        /// Max block size of output data. The max input size for file will be calculated from this parameter and accessible with <see cref="EncryptionBlockSize"/>.
        /// </param>
        public Kes(int maxBlockSize = default)
        {
            if (maxBlockSize > 0)
            {
                if (!ValidateBlockSize(maxBlockSize))
                {
                    throw new ArgumentException("maxBlockSize must be a multiple of " + DefaultDecryptionChunkSize);
                }

                DecryptionBlockSize = maxBlockSize;
                EncryptionBlockSize = ((DecryptionBlockSize / DefaultDecryptionChunkSize) - 1) * DefaultEncryptionChunkSize;
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
            return bs % DefaultDecryptionChunkSize == 0;
        }

        private void ModifyHeader(Header header)
        {
            if ((int)header.DetailLevel > 0)
            {
                header.Version = Version;
                header.EngineVersion = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

                if ((int)header.DetailLevel > 2)
                {
                    header.BlockSize = DecryptionBlockSize;
                } 
            }
        }

        /// <summary>
        /// Encrypts the given data stream and writes the encrypted data to the destination stream.
        /// </summary>
        /// <param name="source">
        /// The stream of the source data with read access.
        /// </param>
        /// <param name="dest">
        /// The stream of destination target with write access.
        /// </param>
        /// <param name="header">
        /// The header to write in the beginning of destination stream. if null, a new header will be created automatically.
        /// </param>
        public async Task EncryptAsync(Stream source, Stream dest, Header header = null)
        {
            Provider.ResetIndex();

            if (header == null)
            {
                header = new Header()
                {
                    DetailLevel = HeaderDetails.Normal
                };
            }

            ModifyHeader(header);
            Provider.ModifyHeader(header);

            var hArray = header.CreatePayload();

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
                Provider.BlockIndex++;
            }

            Provider.ResetIndex();
        }

        /// <summary>
        /// Decrypts the given data stream and writes the decrypted data to the destination data stream.
        /// </summary>
        /// <param name="source">
        /// The stream of encrypted data with read access.
        /// </param>
        /// <param name="dest">
        /// The stream of destination target with write access.
        /// </param>
        public async Task DecryptAsync(Stream source, Stream dest)
        {
            Provider.ResetIndex();

            Header header = Header.ReadHeader<Header>(source);

            if (header.Version != null && header.Version != Version)
            {
                if (header.EngineVersion != null)
                {
                    throw new InvalidOperationException($"Encryptor api version is not supported. You must use kryptor v{header.EngineVersion}");
                }
                else
                {
                    throw new InvalidOperationException("Encryptor api version is not supported.");
                }
            }

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
                Provider.BlockIndex++;
            }

            Provider.ResetIndex();
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
