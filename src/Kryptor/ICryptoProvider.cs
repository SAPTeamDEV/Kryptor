using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides standard interface for encrypt and decrypt data blocks with KES engine.
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// Gets the parent <see cref="KES"/> instance.
        /// </summary>
        KES Parent { get; }

        /// <summary>
        /// Gets the keystore for crypto operations.
        /// </summary>
        KESKeyStore KeyStore { get; }

        /// <summary>
        /// Gets the configuration of continuous encryption method.
        /// </summary>
        bool Continuous { get; }

        /// <summary>
        /// Encrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw data block.</param>
        /// <returns>Encrypted data block.</returns>
        Task<byte[]> EncryptBlockAsync(byte[] data);

        /// <summary>
        /// Decrypts block of data asynchronously.
        /// </summary>
        /// <param name="data">The raw encrypted data block.</param>
        /// <returns>Decrypted data block.</returns>
        Task<byte[]> DecryptBlockAsync(byte[] data);
    }
}
