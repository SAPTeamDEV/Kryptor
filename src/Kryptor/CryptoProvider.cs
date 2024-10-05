using SAPTeam.Kryptor.CryptoProviders;
using SAPTeam.Kryptor.Extensions;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents the abstract base class for KES Crypto Providers.
    /// </summary>
    public abstract class CryptoProvider : ICloneable
    {
        private string registeredIdentifier;
        internal string RegisteredIdentifier
        {
            get => registeredIdentifier;
            set
            {
                if (!string.IsNullOrEmpty(registeredIdentifier)) throw new InvalidOperationException("Cannot change the crypto provider registered id");
                registeredIdentifier = value;
            }
        }

        /// <summary>
        /// Gets the Decryption Chunk Size.
        /// </summary>
        public virtual int DecryptionChunkSize => 512;

        /// <summary>
        /// Gets the Encryption Chunk Size.
        /// </summary>
        public virtual int EncryptionChunkSize => 511;

        private CryptoProviderConfiguration configuration;

        /// <summary>
        /// Gets the configuration of this crypto provider instance.
        /// </summary>
        public CryptoProviderConfiguration Configuration
        {
            get => configuration;
            private set
            {
                string selfId = RegisteredIdentifier;
                if (value.Id != null && CryptoProviderFactory.ResolveId(value.Id) != selfId)
                {
                    throw new InvalidOperationException("Invalid configuration");
                }

                CryptoProviderConfiguration clone = (CryptoProviderConfiguration)value.Clone();
                clone.Id = selfId;
                configuration = clone;
            }
        }

        /// <summary>
        /// Gets the keystore for crypto operations.
        /// </summary>
        public KeyStore KeyStore { get; private set; }

        /// <summary>
        /// Gets a value indicates whether this crypto provider is secure.
        /// </summary>
        /// <remarks>
        /// This value is used in encryption, if the crypto provider is not secure, it only could be used for decryption.
        /// </remarks>
        public virtual bool IsSecure => true;

        /// <summary>
        /// Gets the friendly name of the crypto provider.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Updates the header to include crypto provider data.
        /// </summary>
        /// <param name="header">
        /// The header to modify.
        /// </param>
        protected internal virtual void UpdateHeader(Header header)
        {
            if ((int)header.Verbosity > 2)
            {
                header.Configuration = Configuration;
            }
        }

        /// <summary>
        /// Creates a fork with the same type as the current crypto provider
        /// </summary>
        /// <param name="keyStore"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal CryptoProvider Fork(KeyStore keyStore, CryptoProviderConfiguration configuration = null)
        {
            if (Configuration != null)
            {
                throw new ApplicationException("Broken crypto provider instance storage");
            }

            CryptoProvider clone = Clone() as CryptoProvider;
            clone.Initialize(keyStore, configuration);
            return clone;
        }

        /// <summary>
        /// Initializes a new crypto provider.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize the crypto provider
        /// </param>
        protected virtual void Initialize(KeyStore keyStore, CryptoProviderConfiguration configuration = null)
        {
            KeyStore = keyStore;

            Configuration = configuration ?? new CryptoProviderConfiguration();
        }

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
        public virtual async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process, CancellationToken cancellationToken)
        {
            if (!IsSecure)
            {
                throw new NotSupportedException("Cannot encrypt data with a security compromised crypto provider");
            }

            AesHelper.CheckArgument(data, nameof(data));

            if (process.BlockIndex == 0 && process.ChunkIndex > 0)
            {
                process.ChunkIndex = 0;
            }

            process.BlockHash = Configuration.RemoveHash ? Array.Empty<byte>() : data.Sha256();
            byte[] hash = Configuration.DynamicBlockProcessing ? Transformers.Rotate(process.BlockHash, DynamicEncryption.GetDynamicBlockEntropy(KeyStore, process)) : process.BlockHash;
            List<byte> result = new List<byte>(hash);

            foreach (byte[] chunk in data.Chunk(EncryptionChunkSize))
            {
                IEnumerable<byte> c = await EncryptChunkAsync(chunk, process, cancellationToken);
                result.AddRange(Configuration.DynamicBlockProcessing ? Transformers.Rotate(c.ToArray(), DynamicEncryption.GetDynamicChunkEntropy(KeyStore, process)) : c);
                process.ChunkIndex++;
            }

            return result.ToArray();
        }

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
        public virtual async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process, CancellationToken cancellationToken)
        {
            AesHelper.CheckArgument(data, nameof(data));

            if (process.BlockIndex == 0 && process.ChunkIndex > 0)
            {
                process.ChunkIndex = 0;
            }

            IEnumerable<byte[]> chunks;

            if (Configuration.RemoveHash)
            {
                chunks = data.Chunk(DecryptionChunkSize);
            }
            else
            {
                byte[] _hash = data.Take(32).ToArray();
                process.BlockHash = Configuration.DynamicBlockProcessing ? Transformers.Rotate(_hash, DynamicEncryption.GetDynamicBlockEntropy(KeyStore, process) * -1) : _hash;
                chunks = data.Skip(32).Chunk(DecryptionChunkSize);
            }

            List<byte> result = new List<byte>();

            foreach (byte[] chunk in chunks)
            {
                result.AddRange(await DecryptChunkAsync(Configuration.DynamicBlockProcessing ? Transformers.Rotate(chunk, DynamicEncryption.GetDynamicChunkEntropy(KeyStore, process) * -1) : chunk, process, cancellationToken));
                process.ChunkIndex++;
            }

            byte[] array = result.ToArray();

            return !Configuration.RemoveHash && !process.BlockHash.SequenceEqual(array.Sha256()) ? throw new InvalidDataException("Hash mismatch") : array;
        }

        /// <summary>
        /// Encrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw data chunk.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns>Encrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken);

        /// <summary>
        /// Decrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw encrypted data chunk.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns>Decrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> DecryptChunkAsync(byte[] chunk, CryptoProcess process, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public object Clone() => MemberwiseClone();
    }
}
