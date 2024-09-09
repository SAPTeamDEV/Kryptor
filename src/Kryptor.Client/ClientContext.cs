using System.Reflection;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides an application context with session based design model.
    /// </summary>
    public class ClientContext : Context
    {
        /// <summary>
        /// Gets the version of the kryptor engine.
        /// </summary>
        public static Version EngineVersion { get; } = new Version(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

        /// <summary>
        /// Gets the version of the kryptor client front-end.
        /// </summary>
        public static Version ClientVersion { get; } = new Version(Assembly.GetAssembly(typeof(ClientContext)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

        private readonly CryptoRandom random = new CryptoRandom();

        /// <summary>
        /// Gets the application session host.
        /// </summary>
        public ISessionHost Host { get; private set; }

        /// <inheritdoc/>
        protected override void CreateContext()
        {
            // Just for initializing the EntroX generator.
            byte[] _d = new byte[8];
            random.NextBytes(_d);
            EntroX.AddEntropy(_d);
        }

        /// <inheritdoc/>
        protected override void DisposeContext() => Host?.End(true);

        /// <summary>
        /// Starts a new session host.
        /// </summary>
        /// <param name="host">
        /// An instance of <see cref="ISessionHost"/> to control the program.
        /// </param>
        public void NewSessionHost(ISessionHost host)
        {
            if (Host != null)
            {
                throw new InvalidOperationException("This application already has a session host");
            }

            Host = host;
            StartSessionHost();

            Host.End(false);
            Host = null;
        }

        /// <summary>
        /// Starts the session host
        /// </summary>
        protected virtual void StartSessionHost() => Host.Start(this);
    }
}
