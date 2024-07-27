using System;

using SAPTeam.Kryptor.Client;
using System.Reflection;

#if !NETFRAMEWORK
using ANSIConsole;
#endif

namespace SAPTeam.Kryptor.Cli
{
    public class CliContext : ClientContext
    {
#if DEBUG
        public string CliVersion => Assembly.GetAssembly(typeof(CliSessionHost)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
#else
        public string CliVersion => Utilities.GetShortVersionString(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
#endif

        static CliContext()
        {
#if !NETFRAMEWORK
            if (OperatingSystem.IsWindows() && !ANSIInitializer.Init(false))
            {
                ANSIInitializer.Enabled = false;
            }
#endif
        }
        protected override void CreateContext()
        {
            base.CreateContext();

            Console.CancelKeyPress += delegate
            {
                Dispose();
            };
        }
    }
}