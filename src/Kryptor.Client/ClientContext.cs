using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// Gets the short version string of the kryptor engine.
        /// </summary>
        public string EngineVersion => Utilities.GetShortVersionString(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

        readonly CryptoRandom random = new CryptoRandom();

        /// <summary>
        /// Gets the application session host.
        /// </summary>
        public ISessionHost Host { get; private set; }

        /// <inheritdoc/>
        protected override void CreateContext()
        {
            // Just for initializing the EntroX generator.
            var _d = new byte[8];
            random.NextBytes(_d);
            EntroX.AddEntropy(_d);
        }

        /// <inheritdoc/>
        protected override void DisposeContext()
        {
            Host?.End(true);
        }

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
            Host.Start();

            Host.End(false);
            Host = null;
        }
    }
}
