using System;
using System.Collections.Generic;
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
        /// Gets or sets the block size used for buffer creation in encryption and decryption.
        /// </summary>
        public int BlockSize { get; set; } = 0x8000;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.DecryptBlockAsync"/>.
        /// </summary>
        public int DecryptionBufferSize => BlockSize * Provider.DecryptionChunkSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.EncryptBlockAsync"/>.
        /// </summary>
        public int EncryptionBufferSize => (BlockSize - (Provider.RemoveHash ? 0 : 1)) * Provider.EncryptionChunkSize;

        #endregion

        /// <summary>
        /// Gets the version of the encryptor api backend.
        /// </summary>
        public static Version Version => new Version(0, 12);

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
        public Kes(Header header)
        {
            if (header.BlockSize != null && header.BlockSize > 0)
            {
                BlockSize = (int)header.BlockSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="provider">
        /// The crypto provider.
        /// </param>
        /// <param name="blockSize">
        /// The block size used to read and process data.
        /// </param>
        public Kes(CryptoProvider provider, int blockSize = default) : this(blockSize)
        {
            Provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="blockSize">
        /// The block size used to read and process data.
        /// </param>
        public Kes(int blockSize = default)
        {
            if (blockSize > 0)
            {
                BlockSize = blockSize;
            }
        }

        private void UpdateHeader(Header header)
        {
            if ((int)header.DetailLevel > 0)
            {
                header.Version = Version;
                header.EngineVersion = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

                if ((int)header.DetailLevel > 2)
                {
                    header.BlockSize = BlockSize;
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
            // If there is no header, create a header with normal details.
            if (header == null)
            {
                Dictionary<string, string> extra = new Dictionary<string, string>();
                extra["client"] = "kryptor-core";

                header = new Header()
                {
                    DetailLevel = HeaderDetails.Normal,
                    Extra = extra,
                };
            }

            UpdateHeader(header);
            Provider.UpdateHeader(header);

            if (header.DetailLevel > 0)
            {
                var hArray = header.CreatePayload();
                await dest.WriteAsync(hArray, 0, hArray.Length);
            }

            await ProcessDataAsync(source, dest, EncryptionBufferSize, EncryptBlockAsync);
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

            await ProcessDataAsync(source, dest, DecryptionBufferSize, DecryptBlockAsync);
        }

        private async Task ProcessDataAsync(Stream source, Stream dest, int blockSize, Func<byte[], CryptoProcess, Task<byte[]>> callback)
        {
            CryptoProcess process = new CryptoProcess();
            process.InitializeData();

            double step = (double)((double)blockSize / source.Length) * 100;
            int counter = 1;
            int lastProg = -1;
            OnProgress?.Invoke(0);

            for (long i = 0; i < source.Length; i += blockSize)
            {
                int actualSize = (int)Math.Min(source.Length - source.Position, blockSize);
                byte[] slice = new byte[actualSize];
                await source.ReadAsync(slice, 0, slice.Length);
                var eSlice = await callback(slice, process);
                await dest.WriteAsync(eSlice, 0, eSlice.Length);
                int prog = (int)Math.Round(step * counter);
                if (prog != lastProg)
                {
                    OnProgress?.Invoke(Math.Min(prog, 100));
                    lastProg = prog;
                }
                counter++;
                process.NextBlock(!Provider.Continuous);
            }
        }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <returns>Encrypted data block.</returns>
        public async Task<byte[]> EncryptBlockAsync(byte[] data)
        {
            CryptoProcess process = new CryptoProcess();
            process.InitializeData();
            return await EncryptBlockAsync(data, process);
        }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Encrypted data block.</returns>
        private async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process)
        {
            var result = await Provider.EncryptBlockAsync(data, process);

            return result.Length > DecryptionBufferSize
                ? throw new OverflowException("Resulting buffer size is larger than the allowed size")
                : result;
        }

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data)
        {
            CryptoProcess proc = new CryptoProcess();
            proc.InitializeData();
            return await DecryptBlockAsync(data, proc);
        }

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Decrypted data block.</returns>
        private async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process)
        {
            var result = await Provider.DecryptBlockAsync(data, process);

            return result.Length > EncryptionBufferSize
                ? throw new OverflowException("Resulting buffer size is larger than the allowed size")
                : result;
        }
    }
}
