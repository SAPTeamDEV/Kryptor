using System;
using System.Drawing;
using System.Reflection;

#if !NETFRAMEWORK
using ANSIConsole;
#endif

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliSessionHost : SessionHost
    {
        public CliSessionHost()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        protected virtual void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            
        }

        public override void Start()
        {
            Console.WriteLine($"Kryptor Command-Line Interface v{Program.Context.CliVersion.Color(Color.Cyan)}");
            Console.WriteLine($"Engine version: {Program.Context.EngineVersion.Color(Color.Cyan)}");
        }
    }
}