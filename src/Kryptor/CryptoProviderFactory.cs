// Ignore Spelling: Kryptor

using SAPTeam.Kryptor.CryptoProviders;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents methods to create crypto provider objects.
    /// </summary>
    public static class CryptoProviderFactory
    {
        private static readonly Dictionary<string, CryptoProvider> GlobalProviders = new Dictionary<string, CryptoProvider>();
        private static readonly Dictionary<string, string> GlobalAliases = new Dictionary<string, string>();

        static CryptoProviderFactory()
        {
            RegisterProviderInternal(new StandaloneKey(), "SK", "1");
            RegisterProviderInternal(new TransformedKey(), "TK", "2");
            RegisterProviderInternal(new MixedVector(), "MV", "3");
            RegisterProviderInternal(new TransformedParameters(), "TP", "4");
            RegisterProviderInternal(new DynamicEncryption(), "DE", "5");
        }

        /// <summary>
        /// Registers a new crypto provider given identifier and hints.
        /// </summary>
        /// <param name="instance">
        /// The crypto provider object.
        /// </param>
        /// <param name="prefix">
        /// The preferred prefix for this crypto provider.
        /// </param>
        /// <param name="id">
        /// The preferred identifier for this crypto provider.
        /// </param>
        /// <param name="aliases">
        /// The additional identifiers. Acts like a shortcut. Aliases uses the same prefix as the identifier.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public static void RegisterProvider(CryptoProvider instance, string prefix, string id, string[] aliases)
        {
            prefix = prefix.ToLower();

            if (prefix == "kryptor")
            {
                throw new ArgumentException("The kryptor prefix is only allowed for internal crypto providers");
            }

            RegisterCryptoProviderImpl(instance, prefix, id, aliases);
        }

        internal static void RegisterProviderInternal(CryptoProvider instance, params string[] aliases)
        {
            string prefix = "kryptor";

            RegisterCryptoProviderImpl(instance, prefix, instance.GetType().Name, aliases);
        }

        private static void RegisterCryptoProviderImpl(CryptoProvider instance, string prefix, string id, string[] aliases)
        {
            string name = $"{prefix}:{id}";

            if (GlobalProviders.ContainsKey(name))
            {
                throw new ArgumentException("A provider with this name already registered: " + name);
            }

            instance.RegisteredIdentifier = name;
            GlobalProviders[name] = instance;

            foreach (string alias in aliases)
            {
                string aliasName = $"{prefix}:{alias}";

                if (GlobalAliases.ContainsKey(aliasName))
                {
                    throw new ArgumentException("A provider with this hint already registered: " + alias);
                }

                GlobalAliases[aliasName] = name;
            }
        }

        /// <summary>
        /// Gets all registered crypto providers.
        /// </summary>
        /// <returns>
        /// A Dictionary populated with registered crypto providers.
        /// </returns>
        public static Dictionary<string, CryptoProvider> GetProviders()
        {
            return GlobalProviders.ToDictionary(x => x.Key,
                                                x => (CryptoProvider)x.Value.Clone());
        }

        /// <summary>
        /// Gets all registered crypto provider identifiers.
        /// </summary>
        /// <returns>
        /// An array of all registered crypto providers.
        /// </returns>
        public static string[] GetProviderIds() => GlobalProviders.Keys.ToArray();

        /// <summary>
        /// Gets all aliases associated with the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The id/alias of a registered crypto provider.
        /// </param>
        /// <returns>
        /// An array of all registered aliases.
        /// </returns>
        public static IEnumerable<string> GetAliases(string id)
        {
            id = ResolveId(id);

            foreach (KeyValuePair<string, string> alias in GlobalAliases)
            {
                if (alias.Key == id)
                {
                    yield return alias.Key;
                }
            }
        }

        /// <summary>
        /// Translates given id to absolute identifier.
        /// </summary>
        /// <param name="id">
        /// The id/alias of the registered crypto provider.
        /// </param>
        /// <returns>The absolute identifier of corresponding crypto provider. if given id is already absolute, returns the same id.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static string ResolveId(string id)
        {
            if (!id.Contains(':'))
            {
                id = $"kryptor:{id}";
            }

            return GlobalProviders.ContainsKey(id) ? id : GlobalAliases.TryGetValue(id, out string value) ? value : throw new KeyNotFoundException($"Crypto provider with id {id} is not registered");
        }

        /// <summary>
        /// Gets display name of the registered crypto provider.
        /// </summary>
        /// <param name="id">
        /// The id/alias of a registered crypto provider.
        /// </param>
        /// <returns>
        /// The user-friendly name of the crypto provider.
        /// </returns>
        public static string GetDisplayName(string id)
        {
            string realId = ResolveId(id);

            string result = GlobalProviders[realId].Name;

            if (!realId.StartsWith("kryptor:"))
            {
                result += $" ({realId})";
            }

            return result;
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
            CryptoProvider provider = GlobalProviders[ResolveId(configuration.Id)].Fork(keyStore, configuration);

            return provider;
        }
    }
}
