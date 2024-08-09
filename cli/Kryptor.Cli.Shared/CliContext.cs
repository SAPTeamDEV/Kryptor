using System;

using SAPTeam.Kryptor.Client;
using System.Reflection;
using System.IO;
using System.Drawing;

#if !NETFRAMEWORK
using ANSIConsole;
#endif

namespace SAPTeam.Kryptor.Cli
{
    public class CliContext : ClientContext
    {
        public bool CatchExceptions { get; set; }

        /// <summary>
        /// The root application data folder.
        /// </summary>
        public string ApplicationDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kryptor");

        public string WordlistDirectory => Path.Combine(ApplicationDataDirectory, "Wordlist");

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

            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }

            if (!Directory.Exists(WordlistDirectory))
            {
                Directory.CreateDirectory(WordlistDirectory);
            }
        }

        protected override void StartSessionHost()
        {
            try
            {
                base.StartSessionHost();
            }
            catch (Exception ex)
            {
                if (CatchExceptions)
                {
                    Console.WriteLine($"{ex.GetType().Name.Color(Color.Red)}: {ex.Message}");
                    Environment.Exit(255);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}