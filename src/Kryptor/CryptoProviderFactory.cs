using System;
using System.Collections.Generic;

using SAPTeam.Kryptor.CryptoProviders;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents methods to create crypto provider objects.
    /// </summary>
    public static class CryptoProviderFactory
    {
        private static readonly bool allowKryptorPrefix = true;
        private static Dictionary<string, Type> GlobalProviders = new Dictionary<string, Type>();
        static Dictionary<string, string> GlobalHints = new Dictionary<string, string>();

        static CryptoProviderFactory()
        {
            RegisterProvider<StandaloneKey>("kryptor", "SK", "1");
            RegisterProvider<TransformedKey>("kryptor", "TK", "2");
            RegisterProvider<MixedVector>("kryptor", "MV", "3");
            RegisterProvider<TransformedParameters>("kryptor", "TP", "4");
            RegisterProvider<DynamicEncryption>("kryptor", "DE", "5");
            allowKryptorPrefix = false;
        }

        /// <summary>
        /// Registers a new crypto provider given identifier and hints.
        /// </summary>
        /// <typeparam name="T">
        /// The new crypto provider, must be inherited from <see cref="CryptoProvider"/>.
        /// </typeparam>
        /// <param name="prefix">
        /// The preferred prefix for this crypto provider. The class name will be added to this prefix and creates the identifier, like "my_prefix:my_class".
        /// </param>
        /// <param name="hints">
        /// The additional identifiers. Acts like a shortcut. Hints uses the same prefix.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public static void RegisterProvider<T>(string prefix, params string[] hints)
            where T : CryptoProvider
        {
            if (!allowKryptorPrefix && prefix == "kryptor")
            {
                throw new ArgumentException("The kryptor prefix is only allowed for internal crypto providers");
            }

            Type provider = typeof(T);
            string name = $"{prefix}:{provider.Name}";

            if (!GlobalProviders.ContainsKey(name))
            {
                GlobalProviders[name] = provider;

                foreach (string hint in hints)
                {
                    string hintName = $"{prefix}:{hint}";
                    if (!GlobalHints.ContainsKey(hintName))
                    {
                        GlobalHints.Add(hintName, name);
                    }
                    else
                    {
                        throw new ArgumentException("A provider with this hint already registered: " + hintName);
                    }
                }
            }
            else
            {
                throw new ArgumentException("A provider with this name already registered: " + name);
            }
        }

        static internal string GetRegisteredCryptoProviderId(Type provider)
        {
            if (GlobalProviders.ContainsValue(provider))
            {
                foreach (var item in GlobalProviders)
                {
                    if (item.Value == provider)
                    {
                        return item.Key;
                    }
                }
            }

            throw new ArgumentException("This crypto provider is not registered: " + provider.FullName);
        }

        /// <summary>
        /// Translates given id to absolute identifier.
        /// </summary>
        /// <param name="id">
        /// The id/hint of a registered crypto provider.
        /// </param>
        /// <returns>The absolute identifier of corresponding crypto provider. if given id is already absolute, returns the same id.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static string GetRegisteredCryptoProviderId(string id)
        {
            if (!id.Contains(":"))
            {
                id = $"kryptor:{id}";
            }

            if (GlobalProviders.ContainsKey(id))
            {
                return id;
            }

            if (GlobalHints.ContainsKey(id))
            {
                return GlobalHints[id];
            }

            throw new KeyNotFoundException(id);
        }

        /// <summary>
        /// Translates given id to an absolute identifier and then returns the corresponding <see cref="Type"/> object.
        /// </summary>
        /// <param name="id">
        /// The id/hint of a registered crypto provider.
        /// </param>
        /// <returns>The <see cref="Type"/> object of that crypto provider.</returns>
        public static Type ResolveProviderById(string id)
        {
            return GlobalProviders[GetRegisteredCryptoProviderId(id)];
        }

        /// <summary>
        /// Creates a <see cref="CryptoProvider"/> object to use with KES engine.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore.
        /// </param>
        /// <param name="id">
        /// The id/hint of a registered crypto provider.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public static CryptoProvider Create(KeyStore keyStore, string id)
        {
            CryptoProviderConfiguration configuration = new CryptoProviderConfiguration()
            {
                Id = id,
            };

            return Create(keyStore, configuration);
        }

        /// <summary>
        /// Creates a <see cref="CryptoProvider"/> object to use with KES engine.
        /// </summary>
        /// <param name="keyStore">
        /// The keystore.
        /// </param>
        /// <param name="configuration">
        /// The configuration to initialize crypto provider.
        /// </param>
        /// <returns>
        /// A <see cref="CryptoProvider"/> object to use with KES engine.
        /// </returns>
        public static CryptoProvider Create(KeyStore keyStore, CryptoProviderConfiguration configuration)
        {
            CryptoProvider provider = (CryptoProvider)Activator.CreateInstance(ResolveProviderById(configuration.Id), keyStore, configuration);

            return provider;
        }
    }
}
