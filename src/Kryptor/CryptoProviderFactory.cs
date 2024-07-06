using System;

using SAPTeam.Kryptor.CryptoProviders;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents methods to create crypto provider objects.
    /// </summary>
    public static class CryptoProviderFactory
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
        /// <param name="header">
        /// The header to initialize crypto provider.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public static CryptoProvider Create(CryptoTypes cryptoType, KeyStore keyStore, Header header)
        {
            CryptoProvider provider;

            switch (cryptoType)
            {
                case CryptoTypes.SK:
                    provider = new StandaloneKey(keyStore);
                    break;

                case CryptoTypes.TK:
                    provider = new TransformedKey(keyStore);
                    break;

                case CryptoTypes.MV:
                    provider = new MixedVector(keyStore);
                    break;

                case CryptoTypes.TP:
                    provider = new TransformedParameters(keyStore);
                    break;

                case CryptoTypes.DE:
                    provider = new DynamicEncryption(keyStore);
                    break;

                default:
                    throw new ArgumentException("Invalid crypto type.");
            }

            provider.ApplyHeader(header);
            return provider;
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
        /// <param name="header">
        /// The header to initialize crypto provider.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public static CryptoProvider Create(int cryptoType, KeyStore keyStore, Header header)
        {
            return Create((CryptoTypes)cryptoType, keyStore, header);
        }

        /// <summary>
        /// Creates a <see cref="CryptoProvider"/> object to use with KES engine.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore.
        /// </param>
        /// <param name="header">
        /// The header to initialize crypto provider.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public static CryptoProvider Create(KeyStore keyStore, Header header)
        {
            return Create(header.CryptoType, keyStore, header);
        }
    }
}
