using System;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents methods to create crypto provider objects.
    /// </summary>
    public class CryptoProviderFactory
    {
        /// <summary>
        /// Creates a <see cref="CryptoProvider"/> object to use with KES engine.
        /// </summary>
        /// <param name="cryptoType">
        /// Specifies the crypto provider type to create.
        /// </param>
        /// <param name="keyStore">
        /// The keystore.
        /// </param>
        /// <param name="continuous">
        /// Whether to use continuous encryption method.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public CryptoProvider Create(CryptoTypes cryptoType, KeyStore keyStore, bool continuous = false)
        {
            switch (cryptoType)
            {
                case CryptoTypes.SK:
                    return new StandaloneKeyCryptoProvider(keyStore, continuous);
                default:
                    throw new ArgumentException("Invalid crypto type.");
            }
        }

        /// <summary>
        /// Creates a <see cref="CryptoProvider"/> object to use with KES engine.
        /// </summary>
        /// <param name="cryptoType">
        /// Specifies the crypto provider type to create.
        /// </param>
        /// <param name="keyStore">
        /// The keystore.
        /// </param>
        /// <param name="continuous">
        /// Whether to use continuous encryption method.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public CryptoProvider Create(int cryptoType, KeyStore keyStore, bool continuous = false)
        {
            return Create((CryptoTypes)cryptoType, keyStore, continuous);
        }
    }
}
