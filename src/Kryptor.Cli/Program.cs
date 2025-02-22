﻿using System.CommandLine;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;

using SAPTeam.Kryptor.Helpers;

using Spectre.Console;

namespace SAPTeam.Kryptor.Cli
{
    public class Program
    {
        public static CliContext Context { get; } = CommonTK.Context.Register<CliContext>();

        public static int Main(string[] args)
        {
            RootCommand root = GetRootCommand();

            return root.Invoke(args);
        }

        static GridColumn GetNiceColumn()
        {
            return new GridColumn().NoWrap().PadRight(4);
        }

        public static RootCommand GetRootCommand()
        {
            Option<bool> debugInfo = new Option<bool>("--debug-info", "Show build information");
            debugInfo.AddAlias("-V");

            RootCommand root = new RootCommand("Kryptor Command-Line Interface")
            {
                debugInfo
            };

            GlobalOptionsBinder globalOptionsBinder = GetGlobalOptions(root);

            root.SetHandler((globalOptionsT, verInfoT) =>
            {
                if (verInfoT)
                {
                    DebugInfo();
                }
                else
                {
                    Console.Error.WriteLine("To view help message, run kryptor --help");
                }
            }, globalOptionsBinder, debugInfo);

            #region Common Data Processing Options
            Option<int> blockSize = new Option<int>("--block-size", () => Kes.DefaultBlockSize, "Determines the block size for data processing");
            blockSize.AddAlias("-b");

            Option<string> provider = new Option<string>("--provider", CryptoProviderFactory.GetDefaultProvider, "Determines the crypto provider to process data");
            provider.AddAlias("-p");
            provider.AddValidator(x =>
            {
                try
                {
                    string id = x.Tokens.Count == 0 ? CryptoProviderFactory.GetDefaultProvider() : x.Tokens[0].Value;
                    CryptoProviderFactory.ResolveId(id);
                }
                catch (Exception ex)
                {
                    x.ErrorMessage = ex.Message;
                }
            });

            Option<IEnumerable<string>> parameters = new Option<IEnumerable<string>>(
                name: "--extra-parameters",
                description: "Extra parameters to pass to the crypto provider, comma delimited",
                isDefault: false,
                parseArgument: (x) =>
                {
                    if (x.Tokens.Count == 0)
                    {
                        x.ErrorMessage = "You must specify at least one parameter";
                        return null;
                    }
                    else
                    {
                        return x.Tokens.SelectMany(x => x.Value.Split(','));
                    }
                }
                );
            parameters.AddAlias("-X");
            parameters.AllowMultipleArgumentsPerToken = true;
            parameters.ArgumentHelpName = "parameters";

            Option<bool> continuous = new Option<bool>("--continuous", "Enables using the Continuous method");
            continuous.AddAlias("-c");

            Option<bool> removeHash = new Option<bool>("--remove-hash", "Removes the block hash to increase the security");
            removeHash.AddAlias("-r");

            Option<bool> dbp = new Option<bool>("--dbp", "Enables the Dynamic Block Processing");
            dbp.AddAlias("-d");

            Option<string> outputPath = new Option<string>("--output", getDefaultValue: () => Path.GetFullPath("."), description: "The output directory. by default it points to the current directory.");
            outputPath.AddAlias("-o");

            Option<string> keystore = new Option<string>("--keystore", "Keystore file path or transformer token");
            keystore.AddAlias("-k");
            keystore.IsRequired = true;
            keystore.AddValidator(x =>
            {
                string _inKs = x.Tokens[0].Value;
                if (File.Exists(_inKs) || TransformerToken.IsValid(_inKs))
                {
                    return;
                }
                else
                {
                    x.ErrorMessage = "Invalid token or keystore file not found";
                }
            });

            Argument<IEnumerable<string>> files = new Argument<IEnumerable<string>>(
                name: "files",
                description: "Files or folders to be processed",
                isDefault: false,
                parse: (x) =>
                {
                    if (x.Tokens.Count == 0)
                    {
                        x.ErrorMessage = "You must specify at least one file or folder";
                        return null;
                    }
                    else
                    {
                        return x.Tokens.Select(y => y.Value).ToArray();
                    }
                }
                );
            #endregion

            #region Encryption Options
            Option<int> hVerbose = new Option<int>("--header", () => 2, "Determines the amount of data stored in the header. 0 means no data and 3 means all data needed to decrypt the file (except the keystore)");
            hVerbose.AddAlias("-H");

            Option<string> keyChain = new Option<string>("--keychain", "Determines the key chain json file to store files unique identifiers and keystore information to it");
            keyChain.AddAlias("-K");

            Option<bool> obfuscate = new Option<bool>("--obfuscate", "Obfuscates encrypted file name by renaming it to a random file name");
            obfuscate.AddAlias("-O");

            Command encCmd = new Command("encrypt", "Encrypts files with keystore")
            {
                blockSize,
                provider,
                parameters,
                continuous,
                removeHash,
                dbp,
                hVerbose,
                keyChain,
                obfuscate,
                outputPath,
                keystore,
                files
            };

            encCmd.AddAlias("e");

            encCmd.SetHandler((globalOptionsT, dpoT, hVerboseT, keyChainT, obfuscateT) =>
            {
                EncryptionSessionHost sessionHost = new EncryptionSessionHost(globalOptionsT, dpoT, hVerboseT, keyChainT, obfuscateT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, new DataProcessingOptionsBinder(blockSize, provider, parameters, continuous, removeHash, dbp, outputPath, keystore, files), hVerbose, keyChain, obfuscate);
            #endregion

            #region Decryption Options
            Command decCmd = new Command("decrypt", "Decrypts files with keystore")
            {
                blockSize,
                provider,
                parameters,
                continuous,
                removeHash,
                dbp,
                outputPath,
                keystore,
                files
            };

            decCmd.AddAlias("d");

            decCmd.SetHandler((globalOptionsT, dpoT) =>
            {
                DecryptionSessionHost sessionHost = new DecryptionSessionHost(globalOptionsT, dpoT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, new DataProcessingOptionsBinder(blockSize, provider, parameters, continuous, removeHash, dbp, outputPath, keystore, files));
            #endregion

            Command genCmd = GetGenerateCommand(globalOptionsBinder);

            Command anCmd = GetAnalyzeCommand(globalOptionsBinder, keystore);

            Command wlCmd = GetWordlistCommand(globalOptionsBinder);

            Command kcCmd = GetKeyChainCommand(globalOptionsBinder);

            Command headCmd = GetHeaderCommand(globalOptionsBinder);

            root.AddCommand(anCmd);
            root.AddCommand(encCmd);
            root.AddCommand(decCmd);
            root.AddCommand(genCmd);
            root.AddCommand(headCmd);
            root.AddCommand(kcCmd);
            root.AddCommand(wlCmd);
            return root;
        }

        private static void DebugInfo()
        {
            var mainGrid = new Grid()
                                    .AddColumn(GetNiceColumn())
                                    .AddColumn()
                                    .AddRow("Build Time", BuildInformation.BuildTime.ToLocalTime().ToString("MMM dd, yyyy HH:mm:ss"))
                                    .AddRow("Build Configuration", BuildInformation.Branch.ToString())
                                    .AddRow("Target Framework", BuildInformation.TargetFramework)
                                    .AddRow("Target Platform", BuildInformation.TargetPlatform)
                                    .AddRow("AOT Compiled", $"[{ColorizeBool(BuildInformation.IsAot)}]{BuildInformation.IsAot}[/]")
                                    .AddRow("Application Version", BuildInformation.ApplicationVersion.ToString())
                                    .AddRow("Application Informational Version", BuildInformation.ApplicationInformationalVersion.ToString())
                                    .AddRow("Kryptor Client Utility Version", BuildInformation.ClientVersion.ToString(3))
                                    .AddRow("Kryptor Engine Version", BuildInformation.EngineVersion.ToString(3))
                                    .AddRow("KES API Version", Kes.Version.ToString(2))
                                    .AddRow("KES API Minimum Supported Version", Kes.MinimumSupportedVersion.ToString(2));

            var envGrid = new Grid()
                .AddColumn(GetNiceColumn())
                .AddColumn()
                .AddRow("Async Compatible", $"{AsyncCompat.IsAsyncCompatible}")
                .AddRow("Application data directory", $"{Context.ApplicationDataDirectory}")
                .AddRow("Data directory is writable", $"{Context.ApplicationDataDirectoryIsWritable}");

            var mainPanel = new Panel(mainGrid)
                .Header("Build Info");

            var envPanel = new Panel(envGrid)
                .Header("Environment Info");

            AnsiConsole.Write(new Columns(mainPanel, envPanel));

            Dictionary<string, CryptoProvider> cryptoProviders = CryptoProviderFactory.GetProviders();
            if (cryptoProviders.Count == 0) return;

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("Registered Crypto Providers").LeftJustified());
            AnsiConsole.WriteLine();

            var providerCards = new List<Panel>();

            foreach (KeyValuePair<string, CryptoProvider> provider in cryptoProviders)
            {
                var g = new Grid()
                    .AddColumn(GetNiceColumn())
                    .AddColumn()
                    .AddRow("Aliases", provider.Key.Replace("kryptor:", $"[{Color.DarkCyan}]kryptor:[/]"));

                foreach (var alias in CryptoProviderFactory.GetAliases(provider.Key))
                {
                    g.AddRow("", alias.Replace("kryptor:", $"[{Color.DarkCyan}]kryptor:[/]"));
                }

                g.AddRow("Is Secure", $"[{ColorizeBool(provider.Value.IsSecure)}]{provider.Value.IsSecure}[/]");

                g.AddRow("E&D Chunk Size", $"{provider.Value.EncryptionChunkSize}/{provider.Value.DecryptionChunkSize}");

                providerCards.Add(new Panel(g)
                    .Header(provider.Value.Name));
            }

            AnsiConsole.Write(new Columns(providerCards));
        }

        private static Color ColorizeBool(bool value) => value ? Color.LightGreen : Color.Red;

        private static Command GetHeaderCommand(GlobalOptionsBinder globalOptionsBinder)
        {
            Argument<string> filePath = new Argument<string>("file", "Path of an encrypted file");

            Command headCmd = new Command("header", "Gets informations about an encrypted file")
            {
                filePath
            };

            headCmd.AddAlias("h");

            headCmd.SetHandler((globalOptionsT, filePathT) =>
            {
                HeaderReaderSessionHost sessionHost = new HeaderReaderSessionHost(globalOptionsT, filePathT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, filePath);

            return headCmd;
        }

        private static Command GetKeyChainCommand(GlobalOptionsBinder globalOptionsBinder)
        {
            Argument<string> keyChainPath = new Argument<string>("keychain", "Path of an existing keychain file or folder");
            Argument<string> filePath = new Argument<string>("file", "Path of an encrypted file");

            Command kcCmd = new Command("keychain", "Gets informations about an encrypted file in a keychain file")
            {
                keyChainPath,
                filePath
            };

            kcCmd.AddAlias("k");

            kcCmd.SetHandler((globalOptionsT, keyChainPathT, filePathT) =>
            {
                KeyChainReaderSessionHost sessionHost = new KeyChainReaderSessionHost(globalOptionsT, keyChainPathT, filePathT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, keyChainPath, filePath);

            return kcCmd;
        }

        private static Command GetGenerateCommand(GlobalOptionsBinder globalOptionsBinder)
        {
            Option<KeyStoreGenerator> generateGenerator = new Option<KeyStoreGenerator>(
                            "--random",
                            () =>
                            {
                                return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists("/dev/random")
                                    ? KeyStoreGenerator.Unix
                                    : KeyStoreGenerator.CryptoRng;
                            },
                            "Determines a random key generator to generate keystore in a non-deterministic method.");

            generateGenerator.AddAlias("-r");

            Option<TransformerToken> generateToken = new Option<TransformerToken>(
                "--token",
                parseArgument: (x) =>
                {
                    string _tk = x.Tokens.First().Value;
                    if (TransformerToken.IsValid(_tk))
                    {
                        return TransformerToken.Parse(_tk);
                    }
                    else
                    {
                        x.ErrorMessage = "Invalid tranformer token";
                        return default;
                    }
                },
                description: "Determines the transformer token to generate keystore with a deterministic method");

            generateToken.AddAlias("-t");

            Option<int> generateSize = new Option<int>("--size", "Determines the keystore size. it's recommended to choose sizes larger than 256. This argument is not respected in transformer token generation because the token itself has a size argument.");
            generateSize.AddAlias("-s");

            Option<int> generateMargin = new Option<int>("--margin", "Adds an extra small size to the generated keystore size. it could increase the security of the keystore. this option works in both random and token based generation.");
            generateMargin.AddAlias("-m");

            Option<string> generateOutput = new Option<string>("--output", "Determines the file name of the generated keystore. if no name is specified, the keystore's fingerprint will be used as file name.");
            generateOutput.AddAlias("-o");

            Command genCmd = new Command("generate", "Generates a new keystore")
            {
                generateGenerator,
                generateToken,
                generateSize,
                generateMargin,
                generateOutput
            };

            genCmd.AddAlias("g");

            genCmd.SetHandler((globalOptionsBinderT, generateGeneratorT, generateTokenT, generateSizeT, generateMarginT, generateOutputT) =>
            {
                KeyStoreGenerateSessionHost sessionHost = new KeyStoreGenerateSessionHost(globalOptionsBinderT, generateGeneratorT, generateSizeT, generateTokenT, generateMarginT, generateOutputT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, generateGenerator, generateToken, generateSize, generateMargin, generateOutput);

            return genCmd;
        }

        private static Command GetAnalyzeCommand(GlobalOptionsBinder globalOptionsBinder, Option<string> keystore)
        {
            Option<int> analyzeJobs = new Option<int>("--jobs", "Determines the number of concurrent jobs");
            analyzeJobs.AddAlias("-j");

            Command anCmd = new Command("analyze", "Analyzes the keystore security")
            {
                analyzeJobs,
                keystore
            };

            anCmd.AddAlias("a");

            anCmd.SetHandler((globalOptionsT, analyzeJobsT, keystoreT) =>
            {
                KeyStoreAnalyze.SessionHost sessionHost = new KeyStoreAnalyze.SessionHost(globalOptionsT, analyzeJobsT, keystoreT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, analyzeJobs, keystore);

            return anCmd;
        }

        private static Command GetWordlistCommand(GlobalOptionsBinder globalOptionsBinder)
        {
            Command wlCmd = new Command("wordlist", "Queries the given word in installed wordlists");
            wlCmd.AddAlias("w");

            Argument<string> queryWord = new Argument<string>("word", "The word to query in wordlists");

            Option<string> queryWordlist = new Option<string>("--wordlist", "The wordlist to do query. if not specified, all installed wordlists will be queried.");
            queryWordlist.AddAlias("-w");

            Command wlQuryCmd = new Command("query", "searches for the given word in the wordlists")
            {
                queryWord,
                queryWordlist
            };
            wlQuryCmd.AddAlias("q");

            wlQuryCmd.SetHandler((globalOptionsT, queryWordT, queryWordlistT) =>
            {
                Wordlist.QuerySessionHost sessionHost = new Wordlist.QuerySessionHost(globalOptionsT, queryWordT, queryWordlistT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, queryWord, queryWordlist);

            wlCmd.AddCommand(wlQuryCmd);

            Option<bool> doOptimize = new Option<bool>("--optimize", "Removes all duplicated entries from wordlists. Note: this process requires high amount of ram. if you recieve memory related exceptions, you must disable this option.");
#if !LIGHT
            Argument<string> indexSource = new Argument<string>("source", "The source index v1 file");

            Command wlIndexCmd = new Command("index", "Generates index files (Internal use)")
            {
                doOptimize,
                indexSource
            };

            wlIndexCmd.SetHandler((globalOptionsT, doOptimizeT, indexSourceT) =>
            {
                Wordlist.IndexSessionHost sessionHost = new Wordlist.IndexSessionHost(globalOptionsT, doOptimizeT, indexSourceT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, doOptimize, indexSource);

            wlCmd.AddCommand(wlIndexCmd);

            Option<bool> installList = new Option<bool>("--list", "Lists all available wordlists");
            installList.AddAlias("-l");

            Option<bool> installAll = new Option<bool>("--all", "Installs all available wordlists");
            installAll.AddAlias("-a");

            Option<bool> installRecommended = new Option<bool>("--recommended", "Installs all small-sized wordlists");
            installRecommended.AddAlias("-r");

            Argument<string[]> installIds = new Argument<string[]>("wordlist", "Id of wordlists to install, list of available wordlists could be found by --list option")
            {
                HelpName = "id"
            };

            Command wlInsCmd = new Command("install", "Installs new wordlists")
            {
                installList,
                installAll,
                installRecommended,
                doOptimize,
                installIds
            };

            wlInsCmd.AddAlias("i");

            wlInsCmd.SetHandler((globalOptionsT, installListT, installAllT, installRecommendedT, doOptimizeT, installIdsT) =>
            {
                Wordlist.InstallSessionHost sessionHost = new Wordlist.InstallSessionHost(globalOptionsT, installListT, installAllT, installRecommendedT, doOptimizeT, installIdsT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, installList, installAll, installRecommended, doOptimize, installIds);

            wlCmd.AddCommand(wlInsCmd);
#endif
            Option<bool> removeList = new Option<bool>("--list", "Lists all installed wordlists");
            removeList.AddAlias("-l");

            Option<bool> removeAll = new Option<bool>("--all", "Removes all installed wordlists");
            removeAll.AddAlias("-a");

            Argument<string[]> removeIds = new Argument<string[]>("wordlist", "Id of wordlists to remove, list of all installed wordlists could be found by --list option")
            {
                HelpName = "id"
            };

            Command wlRemCmd = new Command("remove", "Removes installed wordlists")
            {
                removeList,
                removeAll,
                removeIds
            };

            wlRemCmd.AddAlias("r");

            wlRemCmd.SetHandler((globalOptionsT, removeListT, removeAllT, removeIdsT) =>
            {
                Wordlist.RemoveSessionHost sessionHost = new Wordlist.RemoveSessionHost(globalOptionsT, removeListT, removeAllT, removeIdsT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, removeList, removeAll, removeIds);

            wlCmd.AddCommand(wlRemCmd);

            Option<string> importId = new Option<string>("--id", "Determines the wordlist's unique id");
            importId.AddAlias("-i");
            importId.IsRequired = true;

            Option<bool> importEnforce = new Option<bool>("--enforce", "Determines enforcement status of the wordlist. if it's true, it will block any operations if the word is found in the wordlist, but if set to false, it just shows a warning");
            importEnforce.AddAlias("-e");

            Argument<string> importFile = new Argument<string>("file", "The text file to import");

            Command wlImpCmd = new Command("import", "Compiles and imports given file as wordlist")
            {
                importId,
                importEnforce,
                doOptimize,
                importFile
            };

            wlImpCmd.SetHandler((globalOptionsT, importIdT, importEnforceT, doOptimizeT, importFileT) =>
            {
                Wordlist.ImportSessionHost sessionHost = new Wordlist.ImportSessionHost(globalOptionsT, importIdT, importEnforceT, doOptimizeT, importFileT);
                Context.NewSessionHost(sessionHost);
            }, globalOptionsBinder, importId, importEnforce, doOptimize, importFile);

            wlCmd.AddCommand(wlImpCmd);

            return wlCmd;
        }

        private static GlobalOptionsBinder GetGlobalOptions(RootCommand root)
        {
            Option<bool> verbose = new Option<bool>("--verbose", "Shows more detailed informations in console output");
            verbose.AddAlias("-v");
            root.AddGlobalOption(verbose);

            Option<bool> quiet = new Option<bool>("--quiet", "Prevents showing output in console. Only errors and crashes will be shown.");
            quiet.AddAlias("-q");
            root.AddGlobalOption(quiet);

            Option<bool> noColor = new Option<bool>("--no-color", "Disables showing messages with colors");
            root.AddGlobalOption(noColor);

            GlobalOptionsBinder globalOptionsBinder = new GlobalOptionsBinder(verbose, quiet, noColor);

            return globalOptionsBinder;
        }
    }
}
