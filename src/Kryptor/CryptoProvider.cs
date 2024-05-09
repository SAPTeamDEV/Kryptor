using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents the abstract base class for KES Crypto Providers.
    /// </summary>
    public abstract class CryptoProvider : ICryptoProvider
    {
        /// <summary>
        /// Gets the index of the key in the keystore.
        /// </summary>
        protected int index = 0;

        /// <inheritdoc/>
        public KESKeyStore KeyStore { get; protected set; }

        /// <inheritdoc/>
        public bool Continuous { get; protected set; }
        
        /// <inheritdoc/>
        public KES Parent { get; internal set; }

        /// <inheritdoc/>
        public virtual async Task<byte[]> EncryptBlockAsync(byte[] data)
        {
            Check.Argument.IsNotEmpty(data, nameof(data));
            if (data.Length > Parent.EncryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{Parent.EncryptionBlockSize}");
            }

            List<byte> result = new List<byte>(data.Sha256());

            foreach (var chunk in data.Chunk(KES.DefaultEncryptionChunkSize))
            {
                result.AddRange(await EncryptChunkAsync(chunk));
            }

            if (!Continuous)
            {
                index = 0;
            }

            return result.ToArray();
        }

        /// <inheritdoc/>
        public async Task<byte[]> DecryptBlockAsync(byte[] data)
        {
            Check.Argument.IsNotEmpty(data, nameof(data));
            if (data.Length > Parent.DecryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{Parent.DecryptionBlockSize}");
            }

            var chunks = data.Chunk(KES.DefaultDecryptionChunkSize);
            var hash = chunks.First();

            List<byte> result = new List<byte>();

            foreach (var chunk in chunks.Skip(1))
            {
                result.AddRange(await DecryptChunkAsync(chunk));
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

        /// <summary>
        /// Encrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw data chunk.</param>
        /// <returns>Encrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk);

        /// <summary>
        /// Decrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw encrypted data chunk.</param>
        /// <returns>Decrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> DecryptChunkAsync(byte[] chunk);
    }
}
