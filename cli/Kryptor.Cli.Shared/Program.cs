using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

#if !NETFRAMEWORK
using ANSIConsole;
#endif

using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Cli;

using CommandLine;
using System.Diagnostics;
using SAPTeam.Kryptor.Generators;
using System.Runtime.InteropServices;
using System.Threading;
using System.CommandLine;

namespace SAPTeam.Kryptor.Cli
{
    public class Program
    {
        public static CliContext Context { get; private set; }

        public static int Main(string[] args)
        {
            Context = CommonTK.Context.Register<CliContext>();
            return Parse(args);
        }

        static int Parse(string[] args)
        {
            var root = new RootCommand("Kryptor Command-Line Interface");

            var verbose = new Option<bool>("--verbose", "Shows more detailed informations in console output");
            verbose.AddAlias("-v");
            root.AddGlobalOption(verbose);

            #region Common Data Processing Options
            var blockSize = new Option<int>("--block-size", () => Kes.DefaultBlockSize, "Determines the block size for data processing");
            blockSize.AddAlias("-b");

            var provider = new Option<string>("--provider", () => "3", "Determines the crypto provider to process data");
            provider.AddAlias("-p");

            var continuous = new Option<bool>("--continuous", "Enables using the Continuous method");
            continuous.AddAlias("-c");

            var removeHash = new Option<bool>("--remove-hash", "Removes the block hash to increase the security");
            removeHash.AddAlias("-r");

            var dbp = new Option<bool>("--dbp", "Enables the Dynamic Block Processing");
            dbp.AddAlias("-d");

            var keystore = new Option<string>("--keystore", "Keystore file path or transformer token to encrypt/decrypt data");
            keystore.AddAlias("-k");
            keystore.IsRequired = true;

            var files = new Argument<string[]>("files", "Files to be processed");
            #endregion

            #region Encryption Options
            var hVerbose = new Option<int>("--header", () => 2, "Determines the amount of data stored in the header. 0 means no data and 3 means all data needed to decrypt the file (except the keystore)");

            var encCmd = new Command("encrypt", "Encrypts files with keystore")
            {
                blockSize,
                provider,
                continuous,
                removeHash,
                dbp,
                hVerbose,
                keystore,
                files
            };

            encCmd.AddAlias("e");

            encCmd.SetHandler((verboseT, dpoT, hVerboseT) =>
            {
                var sessionHost = new EncryptionSessionHost(verboseT, dpoT, hVerboseT);
                Context.NewSessionHost(sessionHost);
            }, verbose, new DataProcessingOptionsBinder(blockSize, provider, continuous, removeHash, dbp, keystore, files), hVerbose);

            root.AddCommand(encCmd);
            #endregion

            #region Decryption Options
            var decCmd = new Command("decrypt", "Decrypts files with keystore")
            {
                blockSize,
                provider,
                continuous,
                removeHash,
                dbp,
                keystore,
                files
            };

            decCmd.AddAlias("d");

            decCmd.SetHandler((verboseT, dpoT) =>
            {
                var sessionHost = new DecryptionSessionHost(verboseT, dpoT);
                Context.NewSessionHost(sessionHost);
            }, verbose, new DataProcessingOptionsBinder(blockSize, provider, continuous, removeHash, dbp, keystore, files));

            root.AddCommand(decCmd);
            #endregion

            return root.Invoke(args);
        }

        /*
        public static async Task<int> MainOld(string[] args)
        {
#if !NETFRAMEWORK
        if (OperatingSystem.IsWindows() && !ANSIInitializer.Init(false))
        {
            ANSIInitializer.Enabled = false;
        }
#endif

            Console.CancelKeyPress += delegate
            {
                Holder.Token?.Cancel();
                Holder.Task?.Wait();
                Console.WriteLine("Cancelled by user request");
            };

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Arguments opt = GetArguments();
            IsValid();

#if DEBUG
            string appVer = Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
#else
        string appVer = GetVersionString(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
#endif
            Console.WriteLine($"Kryptor Command-Line Interface v{appVer.Color(Color.Cyan)}");

            string engVer = GetVersionString(Assembly.GetAssembly(typeof(Kes)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            Console.WriteLine($"Engine version: {engVer.Color(Color.Cyan)}");

            if (opt.Encrypt || opt.Decrypt)
            {
                CryptoProviderConfiguration configuration = new CryptoProviderConfiguration()
                {
                    Id = opt.Provider,
                    Continuous = opt.Continuous,
                    RemoveHash = opt.RemoveHash,
                    DynamicBlockProccessing = opt.DynamicBlockProccessing,
                };

                KeyStore ks = default;
                Kes kp = InitKES(opt.BlockSize);

                Console.WriteLine($"Using {CryptoProviderFactory.GetDisplayName(opt.Provider).Color(Color.Moccasin)} Crypto Provider");

                if (opt.Decrypt || !opt.CreateKey)
                {
                    ks = ReadKeystore(opt.KeyStore);
                    CryptoProvider skcp = CryptoProviderFactory.Create(ks, configuration);
                    kp.Provider = skcp;
                }

                if (opt.Encrypt)
                {
                    foreach (var file in opt.File)
                    {
                        if (opt.CreateKey)
                        {
                            ks = GenerateKeystore();
                            CryptoProvider skcp = CryptoProviderFactory.Create(ks, configuration);
                            kp.Provider = skcp;
                        }

                        Holder.ProcessTime = Stopwatch.StartNew();
                        Holder.Task = Encrypt(file, kp);
                        await Holder.Task;
                    }
                }
                else
                {
                    foreach (var file in opt.File)
                    {
                        Holder.ProcessTime = Stopwatch.StartNew();
                        Holder.Task = Decrypt(file, kp, ks.Fingerprint.FormatFingerprint());
                        await Holder.Task;
                    }
                }
            }
            else if (opt.Generate)
            {
                Stopwatch sw = Stopwatch.StartNew();
                GenerateKeystore(opt.KeyStore, opt.KeyStoreSize);
                sw.Stop();
#if DEBUG
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms");
#endif
            }

            return Environment.ExitCode;

            async Task Encrypt(string file, Kes kp)
            {
                Console.WriteLine($"Encrypting {Path.GetFileName(file).Color(Color.BurlyWood)}");

                if (!CheckFile(file))
                {
                    return;
                }

                var f = File.OpenRead(file);
                string resolvedName = GetNewFileName(file, file + ".kef");

                Console.WriteLine("Prepairing");
                var f2 = File.OpenWrite(resolvedName);

                Dictionary<string, string> extra = new Dictionary<string, string>
                {
                    ["client"] = "kryptor-cli"
                };

                var header = new CliHeader()
                {
                    Verbosity = HeaderVerbosity.Normal,
                    FileName = Path.GetFileName(file),
                    ClientVersion = new Version(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version),
                    Extra = extra,
                };

                try
                {
                    Holder.Token = new CancellationTokenSource();
                    await kp.EncryptAsync(f, f2, header, Holder.Token.Token);
                }
                catch (OperationCanceledException)
                {
                    f2.Dispose();
                    File.Delete(resolvedName);
                    return;
                }

                f.Dispose();
                f2.Dispose();

                Console.WriteLine($"Saved to {resolvedName.Color(ConsoleColor.Green)}");
            }

            async Task Decrypt(string file, Kes kp, string ksFingerprint)
            {
                Console.WriteLine($"Decrypting {Path.GetFileName(file).Color(Color.BurlyWood)}");

                if (!CheckFile(file))
                {
                    return;
                }

                try
                {
                    var f = File.OpenRead(file);

                    var header = Header.ReadHeader<CliHeader>(f);

                    if (header.Verbosity == HeaderVerbosity.Empty)
                    {
                        Console.WriteLine($"{"Warning:".Color(ConsoleColor.Yellow)} Empty header found, The decryption may be fail");
                    }
#if DEBUG
                    else
                    {
                        Console.WriteLine($"Verbosity: {header.Verbosity}");
                        if (header.Version != null)
                        {
                            Console.WriteLine($"API Version: {header.Version}");
                        }

                        if (header.EngineVersion != null)
                        {
                            Console.WriteLine($"Engine Version: {header.EngineVersion}");
                        }

                        if (header.ClientVersion != null)
                        {
                            Console.WriteLine($"CLI Version: {header.ClientVersion}");
                        }

                        if (header.BlockSize > 0)
                        {
                            Console.WriteLine($"Block Size: {header.BlockSize}");
                        }

                        if (header.Configuration != null)
                        {
                            Console.WriteLine($"Id: {header.Configuration.Id}");
                            Console.WriteLine($"Continuous: {header.Configuration.Continuous}");
                            Console.WriteLine($"Remove Hash: {header.Configuration.RemoveHash}");
                            Console.WriteLine($"Dynamic Block Processing: {header.Configuration.DynamicBlockProccessing}");
                        }

                        if (header.FileName != null)
                        {
                            Console.WriteLine($"File Name: {header.FileName}");
                        }

                        if (header.Extra != null)
                        {
                            Console.WriteLine($"Extra data:\n{string.Join(Environment.NewLine, header.Extra)}");
                        }
                    }
#endif

                    if ((int)header.Verbosity > 0 && header.Version != Kes.Version)
                    {
                        if (header.ClientVersion != null)
                        {
                            Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Encryptor api version is not supported. You must use kryptor cli v{string.Join(".", header.ClientVersion.Major, header.ClientVersion.Minor, header.ClientVersion.Build)}");
                            Environment.ExitCode = 0xFE;
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Encryptor api version is not supported. You must use kryptor v{string.Join(".", header.EngineVersion.Major, header.EngineVersion.Minor, header.EngineVersion.Build)}");
                            Environment.ExitCode = 0xFE;
                            return;
                        }
                    }

                    string origName = header.FileName ?? "decrypted file.dec";
                    string resolvedName = GetNewFileName(file, origName);

                    Console.WriteLine("Prepairing");
                    var f2 = File.OpenWrite(resolvedName);

                    try
                    {
                        Holder.Token = new CancellationTokenSource();
                        await kp.DecryptAsync(f, f2, Holder.Token.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        f2.Dispose();
                        File.Delete(resolvedName);
                        return;
                    }

                    f.Dispose();
                    f2.Dispose();

                    Console.WriteLine($"Saved to {resolvedName.Color(ConsoleColor.Green)}");
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Cannot decrypt file.");
                    Environment.ExitCode = 0xFF;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Cannot decrypt file, File maybe corrupted!");
                    throw;
                }
            }

            Arguments GetArguments()
            {
                var result = Parser.Default.ParseArguments<Arguments>(args);

                if (result == null || result.Value == null)
                {
                    Environment.Exit(1);
                    return null;
                }
                else
                {
                    return result.Value;
                }
            }

            bool CheckFile(string file)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"{"Skipped:".Color(ConsoleColor.DarkGray)} File not found");
                    Environment.ExitCode = 0xFF;
                    return false;
                }

                return true;
            }

            KeyStore ReadKeystore(string keystore)
            {
                TransformerToken token = default;
                bool useToken = false;

                try
                {
                    token = TransformerToken.Parse(keystore);
                    useToken = true;
                }
                catch (ArgumentException)
                {

                }

                if (!File.Exists(keystore) && !useToken)
                {
                    Console.WriteLine($"{"Error:".Color(ConsoleColor.Red)} Keystore not found.");
                    Environment.Exit(3);
                    return default;
                }

                KeyStore ks;

                try
                {
                    if (useToken)
                    {
                        ITranformer tranformer = Transformers.GetTranformer(token);
                        byte[] buffer = new byte[token.KeySize * 32];
                        tranformer.Generate(buffer, token.Rotate);
                        ks = new KeyStore(buffer);
                    }
                    else
                    {
                        Console.WriteLine($"Reading keystore: {Path.GetFileName(keystore).Color(ConsoleColor.DarkYellow)}");
                        ks = new KeyStore(File.ReadAllBytes(keystore));
                    }

                    Console.WriteLine($"Keystore Fingerprint: {ks.Fingerprint.FormatFingerprint().Color(Color.LightSkyBlue)}");
                    return ks;
                }
                catch (FormatException)
                {
                    Console.WriteLine($"{"Error:".Color(ConsoleColor.Red)} Cannot read keystore.");
                    Environment.Exit(3);
                    return default;
                }
            }

            void IsValid()
            {
                if (opt.Encrypt || opt.Decrypt)
                {
                    if (!opt.CreateKey)
                    {
                        if (string.IsNullOrEmpty(opt.KeyStore))
                        {
                            Console.WriteLine($"{"Error:".Color(ConsoleColor.Red)} You must specify keysore file name.");
                            Environment.Exit(2);
                        }
                    }
                    else if (!string.IsNullOrEmpty(opt.KeyStore))
                    {
                        opt.File = opt.File != null ? new string[] { opt.KeyStore }.Concat(opt.File) : (new string[] { opt.KeyStore });
                    }

                    if (opt.File == null || !opt.File.Any())
                    {
                        Console.WriteLine($"{"Error:".Color(ConsoleColor.Red)} You must specify at least one file.");
                        Environment.Exit(2);
                    }
                }
            }

            void ShowProgress(double progress)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, Console.CursorTop);

                if (progress >= 100)
                {
                    Holder.ProcessTime.Stop();
                }

                var rProg = Math.Round(progress);
                var passedTime = Holder.ProcessTime.Elapsed;
                var remTime = progress > 0 ? (Holder.ProcessTime.ElapsedMilliseconds / progress) * (100 - progress) : 0;
                var remainingTime = TimeSpan.FromSeconds(((int)remTime) / 1000);
                Console.WriteLine($"[{(rProg.ToString() + "%").Color(progress < 100 ? Color.Yellow : Color.LawnGreen)}] done in {passedTime.ToString(@"hh\:mm\:ss")} remaining {remainingTime.ToString(@"hh\:mm\:ss")}");
            }

            string GetNewFileName(string path, string origName)
            {
                string destination = Path.Combine(Directory.GetParent(path).FullName, origName);
                int suffix = 2;

                while (File.Exists(destination))
                {
                    string tempName = $"{Path.GetFileNameWithoutExtension(destination)} ({suffix++}){Path.GetExtension(destination)}";

                    if (!File.Exists(Path.Combine(Directory.GetParent(path).FullName, tempName)))
                    {
                        destination = Path.Combine(Directory.GetParent(path).FullName, tempName);
                    }
                }

                return destination;
            }

            string GetVersionString(string verStr)
            {
                var ver = new Version(verStr);
                return string.Join(".", ver.Major, ver.Minor, ver.Build);
            }

            KeyStore GenerateKeystore(string name = "", int keystoreSize = 0)
            {
                KeyStore ks;
                TransformerToken token = default;
                bool useToken = false;

                try
                {
                    token = TransformerToken.Parse(name);

                    ITranformer tranformer = Transformers.GetTranformer(token);
                    Console.WriteLine($"Generating keystore with {token.KeySize.ToString().Color(ConsoleColor.Cyan)} keys with {tranformer.GetType().Name} transformer");

                    byte[] buffer = new byte[token.KeySize * 32];
                    tranformer.Generate(buffer, token.Rotate);
                    ks = new KeyStore(buffer);
                    useToken = true;
                }
                catch (ArgumentException)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine($"{"Error:".Color(ConsoleColor.Red)} Invalid transformer token");
                        Environment.Exit(2);
                    }

                    if (keystoreSize == 0)
                    {
                        keystoreSize = KeyStore.GetRandomOddNumber();
                    }

                    Console.Write($"Generating keystore with {keystoreSize.ToString().Color(ConsoleColor.Cyan)} keys ");

                    byte[] buffer = new byte[keystoreSize * 32];

                    if (opt.EntroX)
                    {
                        Console.WriteLine("using EntroX");
                        new EntroX().Generate(buffer);
                    }
                    else if (opt.CryptoRng)
                    {
                        Console.WriteLine("using CryptoRng");
                        new CryptoRandom().NextBytes(buffer);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || opt.SafeRng)
                    {
                        Console.WriteLine("using SafeRng");
                        new SafeRng().Generate(buffer);
                    }
                    else
                    {
                        Console.WriteLine("using Unix /dev/random");
                        new UnixRandom().Generate(buffer);
                    }

                    ks = new KeyStore(buffer);
                }

                Console.WriteLine($"Keystore Fingerprint: {ks.Fingerprint.FormatFingerprint().Color(Color.LightSkyBlue)}");

                var fName = !string.IsNullOrEmpty(name) && !useToken ? name : BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";

                if (opt.Base64)
                {
                    File.WriteAllText(fName, Convert.ToBase64String(ks.Raw));
                    Console.WriteLine($"Keystore is saved to {fName.Color(ConsoleColor.Green)} as base64");
                }
                else
                {
                    File.WriteAllBytes(fName, ks.Raw);
                    Console.WriteLine($"Keystore is saved to {fName.Color(ConsoleColor.Green)}");
                }

                return ks;
            }

            Kes InitKES(int blockSize)
            {
                Kes kp = new Kes(blockSize);
                kp.OnProgress += ShowProgress;
                return kp;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"{e.ExceptionObject.GetType().Name.Color(Color.Red)}: {((Exception)e.ExceptionObject).Message}");
#if RELEASE
        Environment.Exit(255);
#endif
        }
        */
    }
}
