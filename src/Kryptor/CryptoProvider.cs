using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EnsureThat;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents the abstract base class for KES Crypto Providers.
    /// </summary>
    public abstract class CryptoProvider
    {
        /// <summary>
        /// Gets the index of the key in the keystore.
        /// </summary>
        protected int index = 0;

        /// <summary>
        /// Gets the keystore for crypto operations.
        /// </summary>
        public KeyStore KeyStore { get; protected set; }

        /// <summary>
        /// Gets the configuration of continuous encryption method.
        /// </summary>
        public bool Continuous { get; protected set; }

        /// <summary>
        /// Gets the parent <see cref="KES"/> instance.
        /// </summary>
        public KES Parent { get; internal set; }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <returns>Encrypted data block.</returns>
        public virtual async Task<byte[]> EncryptBlockAsync(byte[] data)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            if (data.Length > Parent.EncryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{Parent.EncryptionBlockSize}");
            }

            byte[] hash = data.Sha256();
            List<byte> result = new List<byte>(hash);

            foreach (var chunk in data.Chunk(KES.DefaultEncryptionChunkSize))
            {
                result.AddRange(await EncryptChunkAsync(chunk, hash));
            }

            if (!Continuous)
            {
                index = 0;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <returns>Decrypted data block.</returns>
        public async Task<byte[]> DecryptBlockAsync(byte[] data)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            if (data.Length > Parent.DecryptionBlockSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{Parent.DecryptionBlockSize}");
            }

            var chunks = data.Chunk(KES.DefaultDecryptionChunkSize);
            var hash = chunks.First();

            List<byte> result = new List<byte>();

            foreach (var chunk in chunks.Skip(1))
            {
                result.AddRange(await DecryptChunkAsync(chunk, hash));
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
        /// <param name="hash">The SHA256 hash of the parent data block.</param>
        /// <returns>Encrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, byte[] hash);

        /// <summary>
        /// Decrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw encrypted data chunk.</param>
        /// <param name="hash">The SHA256 hash of the parent data block.</param>
        /// <returns>Decrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> DecryptChunkAsync(byte[] chunk, byte[] hash);

        /// <summary>
        /// Modifies the header to include crypto provider data.
        /// </summary>
        /// <param name="header">
        /// The header to modify.
        /// </param>
        protected internal abstract void ModifyHeader(Header header);

        internal void ResetIndex()
        {
            index = 0;
        }
    }
}
