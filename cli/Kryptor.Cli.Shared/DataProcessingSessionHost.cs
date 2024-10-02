using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingSessionHost : CliSessionHost
    {
        private readonly string[] fString;

        public int BlockSize { get; }

        public CryptoProviderConfiguration Configuration { get; }

        public string OutputPath { get; }

        public KeyStore KeyStore { get; private set; }

        public readonly string KeystoreString;

        public DataProcessingSessionHost(GlobalOptions globalOptions, DataProcessingOptions options) : base(globalOptions)
        {
            BlockSize = options.BlockSize;

            Configuration = new CryptoProviderConfiguration()
            {
                Id = CryptoProviderFactory.ResolveId(options.Provider),
                Continuous = options.Continuous,
                RemoveHash = options.RemoveHash,
                DynamicBlockProccessing = options.DynamicBlockProcessing,
            };

            OutputPath = Utilities.EnsureDirectoryExists(options.OutputPath);

            fString = options.Files;

            KeystoreString = options.KeyStore;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStore = LoadKeyStore(KeystoreString);
        }

        public Task GetSmartProgress(SessionGroup sessionGroup)
        {
            Task progress = sessionGroup.Count > 20 ? ShowProgressMonitored(sessionGroup) : ShowProgressMonitored(true);
            return progress;
        }

        public void EnumerateFiles(Action<string, string> action)
        {
            Parallel.ForEach(fString, file =>
            {
                if (Directory.Exists(file))
                {
                    foreach (string subfile in Directory.GetFiles(file, "*", SearchOption.AllDirectories))
                    {
                        action(subfile, Utilities.EnsureDirectoryExists(Path.Combine(OutputPath, GetRelativePath(file, Path.GetDirectoryName(subfile)))));
                    }
                }
                else
                {
                    action(Path.GetFullPath(file), OutputPath);
                }
            });
        }

        public static string GetRelativePath(string relativeTo, string path) =>
#if NET6_0_OR_GREATER
            Path.GetRelativePath(relativeTo, path);
#else
            RelativePathLegacy(relativeTo, path);
#endif

        private static string RelativePathLegacy(string relativeTo, string path)
        {
            if (Directory.Exists(path) && !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar;
            }

            Uri pathUri = new Uri(path);
            // Folders must end in a slash
            if (!relativeTo.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativeTo += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(relativeTo);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}