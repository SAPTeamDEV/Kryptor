using System.Data;
using System.Reflection;

using SAPTeam.Kryptor.CryptoProviders;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides methods to encrypt and decrypt data using the Kryptor Encryption Standard (KES).
    /// </summary>
    public class Kes : IProgressReport
    {
        #region Size Parameters

        /// <summary>
        /// Gets the default value for block size.
        /// </summary>
        public const int DefaultBlockSize = 0x8000;

        /// <summary>
        /// Gets or sets the block size used for buffer creation in encryption and decryption.
        /// </summary>
        public int BlockSize { get; set; } = DefaultBlockSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.DecryptBlockAsync"/>.
        /// </summary>
        public int GetDecryptionBufferSize(CryptoProcess process = default) => (process.BlockSize > 0 ? process.BlockSize : BlockSize) * Provider.DecryptionChunkSize;

        /// <summary>
        /// Gets max input buffer size for <see cref="CryptoProvider.EncryptBlockAsync"/>.
        /// </summary>
        public int GetEncryptionBufferSize(CryptoProcess process = default) => ((process.BlockSize > 0 ? process.BlockSize : BlockSize) * Provider.EncryptionChunkSize) - (Provider.Configuration.RemoveHash ? 0 : 32);

        #endregion

        /// <summary>
        /// Gets the version of the kes encryptor.
        /// </summary>
        public static Version Version => new Version(0, 18, 0, 0);

        /// <summary>
        /// Gets the minimum supported version of the kes decryptor.
        /// </summary>
        public static Version MinimumSupportedVersion => new Version(0, 18, 0, 0);

        /// <summary>
        /// Gets or sets the crypto provider.
        /// </summary>
        public CryptoProvider Provider { get; set; }

        /// <inheritdoc/>
        public event EventHandler<double> ProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class with a new instance of the requested crypto provider.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize crypto provider.
        /// </param>
        /// <param name="blockSize">
        /// The block size used to read and process data.
        /// </param>
        public Kes(KeyStore keyStore, CryptoProviderConfiguration configuration, int blockSize = default) : this(blockSize) => Provider = CryptoProviderFactory.Create(keyStore, configuration);

        /// <summary>
        /// Initializes a new instance of the <see cref="Kes"/> class.
        /// </summary>
        /// <param name="provider">
        /// The crypto provider.
        /// </param>
        /// <param name="blockSize">
        /// The block size used to read and process data.
        /// </param>
        public Kes(CryptoProvider provider, int blockSize = default) : this(blockSize) => Provider = provider;

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
            if ((int)header.Verbosity > 0)
            {
                header.Version = Version;
                header.EngineVersion = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

                if ((int)header.Verbosity > 1)
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
        public async Task EncryptAsync(Stream source, Stream dest) => await EncryptAsync(source, dest, null, CancellationToken.None);

        /// <summary>
        /// Encrypts the given data stream and writes the encrypted data to the destination stream.
        /// </summary>
        /// <param name="source">
        /// The stream of the source data with read access.
        /// </param>
        /// <param name="dest">
        /// The stream of destination target with write access.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public async Task EncryptAsync(Stream source, Stream dest, CancellationToken cancellationToken) => await EncryptAsync(source, dest, null, cancellationToken);

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
        public async Task EncryptAsync(Stream source, Stream dest, Header header) => await EncryptAsync(source, dest, header, CancellationToken.None);

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
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public async Task EncryptAsync(Stream source, Stream dest, Header header, CancellationToken cancellationToken)
        {
            // If there is no header, create a header with normal details.
            if (header == null)
            {
                Dictionary<string, string> extra = new Dictionary<string, string>
                {
                    ["client"] = "kryptor-core"
                };

                header = new Header()
                {
                    Verbosity = HeaderVerbosity.Normal,
                    Extra = extra,
                };
            }

            UpdateHeader(header);
            Provider.UpdateHeader(header);

            if (header.Verbosity > 0)
            {
                byte[] hArray = header.CreatePayload();
                await AsyncCompat.WriteAsync(dest, hArray, 0, hArray.Length, cancellationToken);
            }

            await ProcessDataAsync(source, dest, true, cancellationToken);
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
        public async Task DecryptAsync(Stream source, Stream dest) => await DecryptAsync(source, dest, CancellationToken.None);

        /// <summary>
        /// Decrypts the given data stream and writes the decrypted data to the destination data stream.
        /// </summary>
        /// <param name="source">
        /// The stream of encrypted data with read access.
        /// </param>
        /// <param name="dest">
        /// The stream of destination target with write access.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public async Task DecryptAsync(Stream source, Stream dest, CancellationToken cancellationToken)
        {
            Header header = Header.ReadHeader<Header>(source);

            if (header.Version != null && (header.Version < MinimumSupportedVersion || header.Version > Version))
            {
                if (header.EngineVersion != null)
                {
                    throw new InvalidOperationException($"Encryptor api version is not supported. You must use kryptor engine v{header.EngineVersion}");
                }
                else
                {
                    throw new InvalidOperationException("Encryptor api version is not supported.");
                }
            }

            await ProcessDataAsync(source, dest, false, cancellationToken);
        }

        private async Task ProcessDataAsync(Stream source, Stream dest, bool doEncrypt, CancellationToken cancellationToken)
        {
            CryptoProcess process = new CryptoProcess();
            process.InitializeData();

            Func<CryptoProcess, int> blockSizeCallback;
            Func<byte[], CryptoProcess, Task<byte[]>> cryptoCallback;
            int chunckSize;

            if (doEncrypt)
            {
                blockSizeCallback = GetEncryptionBufferSize;
                cryptoCallback = EncryptBlockAsync;
                chunckSize = Provider.EncryptionChunkSize;
            }
            else
            {
                blockSizeCallback = GetDecryptionBufferSize;
                cryptoCallback = DecryptBlockAsync;
                chunckSize = Provider.DecryptionChunkSize;
            }

            double step = (double)((double)chunckSize / (source.Length - source.Position)) * 100;
            int counter = 0;
            int blockSize;
            long i = 0;
            ProgressChanged?.Invoke(this, 0);

            try
            {
                while (i < source.Length)
                {
                    process.BlockSize = Provider.Configuration.DynamicBlockProcessing ? DynamicEncryption.GetDynamicBlockSize(Provider.KeyStore, process) : BlockSize;
                    blockSize = blockSizeCallback(process);
                    int actualSize = (int)Math.Min(source.Length - source.Position, blockSize);

                    byte[] slice = new byte[actualSize];
                    await AsyncCompat.ReadAsync(source, slice, 0, slice.Length, cancellationToken);
                    byte[] eSlice = await cryptoCallback(slice, process);
                    await AsyncCompat.WriteAsync(dest, eSlice, 0, eSlice.Length, cancellationToken);

                    counter += blockSize / chunckSize;
                    double prog = step * counter;
                    ProgressChanged?.Invoke(this, Math.Min(prog, 100));

                    process.NextBlock(!Provider.Configuration.Continuous);
                    i += blockSize;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new DataException("Small keystore size");
            }
        }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <returns>Encrypted data block.</returns>
        public async Task<byte[]> EncryptBlockAsync(byte[] data) => await EncryptBlockAsync(data, CancellationToken.None);

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns>Encrypted data block.</returns>
        public async Task<byte[]> EncryptBlockAsync(byte[] data, CancellationToken cancellationToken)
        {
            CryptoProcess process = new CryptoProcess();
            process.InitializeData();
            return await EncryptBlockAsync(data, process, cancellationToken);
        }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Encrypted data block.</returns>
        public async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process) => await EncryptBlockAsync(data, process, CancellationToken.None);

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns>Encrypted data block.</returns>
        public async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process, CancellationToken cancellationToken)
        {
            if (data.Length > GetEncryptionBufferSize(process))
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{GetEncryptionBufferSize(process)}");
            }

            byte[] result = await Provider.EncryptBlockAsync(data, process, cancellationToken);

            return result.Length > GetDecryptionBufferSize(process)
                ? throw new OverflowException("Resulting buffer size is larger than the allowed size")
                : result;
        }

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data) => await DecryptBlockAsync(data, CancellationToken.None);

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data, CancellationToken cancellationToken)
        {
            CryptoProcess proc = new CryptoProcess();
            proc.InitializeData();
            return await DecryptBlockAsync(data, proc, cancellationToken);
        }

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process) => await DecryptBlockAsync(data, process, CancellationToken.None);

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process, CancellationToken cancellationToken)
        {
            if (data.Length > GetDecryptionBufferSize(process))
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{GetDecryptionBufferSize(process)}");
            }

            byte[] result = await Provider.DecryptBlockAsync(data, process, cancellationToken);

            return result.Length > GetEncryptionBufferSize(process)
                ? throw new OverflowException("Resulting buffer size is larger than the allowed size")
                : result;
        }
    }
}
