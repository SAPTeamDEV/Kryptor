using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingSessionHost : CliSessionHost
    {
        public int BlockSize { get; }

        public CryptoProviderConfiguration Configuration { get; }

        public string OutputPath { get; }

        public KeyStore KeyStore { get; private set; }

        public readonly string KeystoreString;

        public Dictionary<string, string> Files { get; }

        public DataProcessingSessionHost(GlobalOptions globalOptions, DataProcessingOptions options) : base(globalOptions)
        {
            BlockSize = options.BlockSize;

            Configuration = new CryptoProviderConfiguration()
            {
                Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(options.Provider),
                Continuous = options.Continuous,
                RemoveHash = options.RemoveHash,
                DynamicBlockProccessing = options.DynamicBlockProcessing,
            };

            OutputPath = Utilities.EnsureDirectoryExists(options.OutputPath);

            var fDict = new Dictionary<string, string>();
            foreach (var file in options.Files)
            {
                if (Directory.Exists(file))
                {
                    foreach (var subfile in Directory.GetFiles(file, "*", SearchOption.AllDirectories))
                    {
                        fDict[subfile] = Utilities.EnsureDirectoryExists(Path.Combine(OutputPath, GetRelativePath(file, Path.GetDirectoryName(subfile))));
                    }
                }
                else
                {
                    fDict[Path.GetFullPath(file)] = OutputPath;
                }
            }

            Files = fDict;

            KeystoreString = options.KeyStore;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStore = LoadKeyStore(KeystoreString);
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
#if NET6_0_OR_GREATER
            return Path.GetRelativePath(relativeTo, path);
#else
            return RelativePathLegacy(relativeTo, path);
#endif
        }

        static string RelativePathLegacy(string relativeTo, string path)
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