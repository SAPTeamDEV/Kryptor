using System.Drawing;
using System.Reflection;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public partial class CliContext : ClientContext
    {
        public bool CatchExceptions { get; set; }

        public bool NoColor { get; set; }

        /// <summary>
        /// The root application data folder.
        /// </summary>
        public string ApplicationDataDirectory { get; }

        public bool ApplicationDataDirectoryIsWritable
        {
            get
            {
                try
                {
                    var f = File.Open(Path.Combine(ApplicationDataDirectory, ".write_test"), FileMode.Create, FileAccess.Write);
                    var buffer = new byte[] { 79, 75 };
                    f.Write(buffer, 0, buffer.Length);
                    f.Flush();
                    f.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string WordlistDirectory { get; }

#if DEBUG
        public string CliVersion => Assembly.GetAssembly(typeof(CliSessionHost)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
#else
        public string CliVersion => Utilities.GetShortVersionString(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
#endif

        public CliContext()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string unixHome = Environment.GetEnvironmentVariable("HOME") ?? Path.GetFullPath(".");
            string altAppData = Path.Combine(unixHome, ".config");

            ApplicationDataDirectory = Path.Combine(string.IsNullOrEmpty(localAppData) ? altAppData : localAppData, "Kryptor".ToLowerIfUnix());
            WordlistDirectory = Path.Combine(ApplicationDataDirectory, "Wordlist".ToLowerIfUnix());
        }

        protected override void CreateContext()
        {
            base.CreateContext();

            if (BuildInformation.Variant != BuildVariant.Android)
            {
                Console.CancelKeyPress += delegate
                {
                    Dispose();
                };
            }

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
                    Console.Error.WriteLine($"{ex.GetType().Name.WithColor(Color.Red)}: {ex.Message}");
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