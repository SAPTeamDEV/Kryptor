﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Gets the Decryption Chunk Size.
        /// </summary>
        public readonly int DecryptionChunkSize = 32;

        /// <summary>
        /// Gets the Encryption Chunk Size.
        /// </summary>
        public readonly int EncryptionChunkSize = 31;

        /// <summary>
        /// Gets the name of the crypto provider.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the configuration of continuous encryption method.
        /// </summary>
        public virtual bool Continuous { get; protected set; }

        /// <summary>
        /// Gets the configuration of remove hash feature.
        /// </summary>
        public virtual bool RemoveHash { get; protected set; }

        /// <summary>
        /// Gets the parent <see cref="Kes"/> instance.
        /// </summary>
        protected internal Kes Parent { get; internal set; }

        /// <summary>
        /// Gets the keystore for crypto operations.
        /// </summary>
        protected KeyStore KeyStore { get; private set; }

        /// <summary>
        /// Initializes a new crypto provider.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore with at least 2 keys.
        /// </param>
        /// <param name="continuous">
        /// Whether to use continuous encryption method.
        /// </param>
        /// <param name="removeHash">
        /// Whether to remove block hashes.
        /// </param>
        protected CryptoProvider(KeyStore keyStore, bool continuous = false, bool removeHash = false)
        {
            KeyStore = keyStore;
            Continuous = continuous;
            RemoveHash = removeHash;
        }

        /// <summary>
        /// Applies the header properties to the crypto provider.
        /// </summary>
        /// <param name="header">
        /// The header to apply properties.
        /// </param>
        protected internal virtual void ApplyHeader(Header header)
        {
            if (header.Continuous != null)
            {
                Continuous = (bool)header.Continuous;
            }
            if (header.RemoveHash != null)
            {
                RemoveHash = (bool)header.RemoveHash;
            }
        }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Encrypted data block.</returns>
        public virtual async Task<byte[]> EncryptBlockAsync(byte[] data, CryptoProcess process)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            if (data.Length > Parent.EncryptionBufferSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{Parent.EncryptionBufferSize}");
            }

            if (process.BlockIndex == 0 && process.ChunkIndex > 0)
            {
                process.ChunkIndex = 0;
            }

            byte[] hash = process.BlockHash = RemoveHash ? Array.Empty<byte>() : data.Sha256();
            List<byte> result = new List<byte>(hash);

            foreach (var chunk in data.Chunk(EncryptionChunkSize))
            {
                result.AddRange(await EncryptChunkAsync(chunk, process));
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
        /// <returns>Decrypted data block.</returns>
        public virtual async Task<byte[]> DecryptBlockAsync(byte[] data, CryptoProcess process)
        {
            Ensure.Enumerable.HasItems(data, nameof(data));
            if (data.Length > Parent.DecryptionBufferSize)
            {
                throw new ArgumentException($"Max allowed size for input buffer is :{Parent.DecryptionBufferSize}");
            }

            if (process.BlockIndex == 0 && process.ChunkIndex > 0)
            {
                process.ChunkIndex = 0;
            }

            IEnumerable<byte[]> chunks;

            if (RemoveHash)
            {
                chunks = data.Chunk(DecryptionChunkSize);
            }
            else
            {
                process.BlockHash = data.Take(32).ToArray();
                chunks = data.Skip(32).Chunk(DecryptionChunkSize);
            }

            List<byte> result = new List<byte>();

            foreach (var chunk in chunks)
            {
                result.AddRange(await DecryptChunkAsync(chunk, process));
                process.ChunkIndex++;
            }

            var array = result.ToArray();

            return !RemoveHash && !process.BlockHash.SequenceEqual(array.Sha256()) ? throw new InvalidDataException("Hash mismatch") : array;
        }

        /// <summary>
        /// Encrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw data chunk.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Encrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> EncryptChunkAsync(byte[] chunk, CryptoProcess process);

        /// <summary>
        /// Decrypts chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The raw encrypted data chunk.</param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns>Decrypted data chunk.</returns>
        protected abstract Task<IEnumerable<byte>> DecryptChunkAsync(byte[] chunk, CryptoProcess process);

        /// <summary>
        /// Updates the header to include crypto provider data.
        /// </summary>
        /// <param name="header">
        /// The header to modify.
        /// </param>
        protected internal virtual void UpdateHeader(Header header)
        {
            if ((int)header.DetailLevel > 1)
            {
                header.Fingerprint = KeyStore.Fingerprint;
            }

            if ((int)header.DetailLevel > 2)
            {
                header.Continuous = Continuous;
                header.RemoveHash = RemoveHash;
            }
        }
    }
}
