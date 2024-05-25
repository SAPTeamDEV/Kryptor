using System.Drawing;
using System.Reflection;

using ANSIConsole;

using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Cli;

if (OperatingSystem.IsWindows() && !ANSIInitializer.Init(false))
{
    ANSIInitializer.Enabled = false;
}

Arguments opt = GetArguments();
IsValid();

string appVer = GetVersionString(Assembly.GetAssembly(typeof(Program)));
Console.WriteLine($"Kryptor Command-Line Interface v{appVer.Color(Color.Cyan)}");

string engVer = GetVersionString(Assembly.GetAssembly(typeof(Kes)));
Console.WriteLine($"Engine version: {engVer.Color(Color.Cyan)}");

if (opt.Encrypt || opt.Decrypt)
{
    CLIHeader paramHeader = new CLIHeader()
    {
        CryptoType = opt.Provider,
        BlockSize = opt.BlockSize,
        Continuous = opt.Continuous,
        RemoveHash = opt.RemoveHash,
    };

    KeyStore ks = default;
    Kes kp = InitKES(opt, paramHeader);

    if ((int)opt.Provider > 0 && (int)opt.Provider < 3)
    {
        Console.WriteLine($"Using {opt.Provider.ToString().Color(Color.LightSalmon)} Crypto Provider");
        Console.WriteLine($"{"Warning:".Color(ConsoleColor.Yellow)} You are using a vulnerable crypto provider, it's recommended to use MV or DE as Crypto Provider.");
    }
    else
    {
        Console.WriteLine($"Using {opt.Provider.ToString().Color(Color.Moccasin)} Crypto Provider");
    }

    if (opt.Decrypt || !opt.CreateKey)
    {
        ks = ReadKeystore(opt.KeyStore);
        CryptoProvider skcp = CryptoProviderFactory.Create(ks, paramHeader);
        kp.Provider = skcp;
    }

    if (opt.Encrypt)
    {
        foreach (var file in opt.File)
        {
            if (opt.CreateKey)
            {
                ks = GenerateKeystore();
                CryptoProvider skcp = CryptoProviderFactory.Create(ks, paramHeader);
                kp.Provider = skcp;
            }

            Holder.ProcessTime = DateTime.Now;
            await Encrypt(file, kp);
        }
    }
    else
    {
        foreach (var file in opt.File)
        {
            Holder.ProcessTime = DateTime.Now;
            await Decrypt(file, kp, ks.Fingerprint.FormatFingerprint());
        }
    }
}
else if (opt.Generate)
{
    GenerateKeystore(opt.KeyStore, opt.KeyStoreSize);
}

return Environment.ExitCode;

async Task Encrypt(string file, Kes kp)
{
    Console.WriteLine($"Encrypting {Path.GetFileName(file).Color(Color.BurlyWood)}");

    if (!CheckFile(file))
    {
        return;
    }

    using var f = File.OpenRead(file);
    string resolvedName = GetNewFileName(file, file + ".kef");

    Console.WriteLine("Prepairing");
    using var f2 = File.OpenWrite(resolvedName);

    Dictionary<string, string> extra = new Dictionary<string, string>();
    extra["client"] = "kryptor-cli";

    var header = new CLIHeader()
    {
        DetailLevel = HeaderDetails.Normal,
        OriginalName = Path.GetFileName(file),
        CliVersion = new Version(appVer),
        Extra = extra,
    };

    await kp.EncryptAsync(f, f2, header);

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
        using var f = File.OpenRead(file);

        var header = Header.ReadHeader<CLIHeader>(f);

        if (header.DetailLevel == HeaderDetails.Empty)
        {
            Console.WriteLine($"{"Warning:".Color(ConsoleColor.Yellow)} Empty header found, The decryption may be fail");
        }
#if DEBUG
        else
        {
            Console.WriteLine($"Detail Level: {header.DetailLevel}");
            if (header.Version != null)
            {
                Console.WriteLine($"API Version: {header.Version}");
            }

            if (header.EngineVersion != null)
            {
                Console.WriteLine($"Engine Version: {header.EngineVersion}");
            }

            if (header.CliVersion != null)
            {
                Console.WriteLine($"CLI Version: {header.CliVersion}");
            }

            if ((int)header.CryptoType > 0)
            {
                Console.WriteLine($"Crypto Type: {header.CryptoType}");
            }

            if (header.BlockSize != null)
            {
                Console.WriteLine($"Block Size: {header.BlockSize}");
            }

            if (header.Continuous != null)
            {
                Console.WriteLine($"Continuous: {header.Continuous}");
            }

            if (header.OriginalName != null)
            {
                Console.WriteLine($"Original Name: {header.OriginalName}");
            }

            if (header.Extra != null)
            {
                Console.WriteLine($"Extra data:\n{string.Join(Environment.NewLine, header.Extra)}");
            }
        }
#endif

        if ((int)header.DetailLevel > 0 && header.Version != Kes.Version)
        {
            if (header.CliVersion != null)
            {
                Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Encryptor api version is not supported. You must use kryptor cli v{header.CliVersion}");
                Environment.ExitCode = 0xFE;
                return;
            }
            else
            {
                Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Encryptor api version is not supported. You must use kryptor v{header.EngineVersion}");
                Environment.ExitCode = 0xFE;
                return;
            }
        }

        if ((int)header.DetailLevel > 1)
        {
            string fingerprint = header.Fingerprint.FormatFingerprint();
#if DEBUG
            Console.WriteLine($"File Fingerprint: {fingerprint.Color(Color.LightGoldenrodYellow)}");
#endif
            if (fingerprint != ksFingerprint)
            {
                Console.WriteLine($"{"Failed:".Color(ConsoleColor.Red)} Cannot decrypt file with this keystore.");
                Environment.ExitCode = 0xFE;
                return;
            }
        }

        string origName = header.OriginalName ?? "decrypted file.dec";
        string resolvedName = GetNewFileName(file, origName);

        Console.WriteLine("Prepairing");
        using var f2 = File.OpenWrite(resolvedName);

        await kp.DecryptAsync(f, f2);

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
        Environment.ExitCode = 0xFF;
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
            opt.File = opt.File != null ? new string[] { opt.KeyStore }.Concat(opt.File) : (IEnumerable<string>)(new string[] { opt.KeyStore });
        }

        if (opt.File == null || !opt.File.Any())
        {
            Console.WriteLine($"{"Error:".Color(ConsoleColor.Red)} You must specify at least one file.");
            Environment.Exit(2);
        }
    }
}

void ShowProgress(int progress)
{
    Console.SetCursorPosition(0, Console.CursorTop - 1);
    Console.Write(new string(' ', Math.Min(10, Console.BufferWidth)));
    Console.SetCursorPosition(0, Console.CursorTop);
    Console.WriteLine(progress < 100 ? $"{(progress.ToString() + "%").Color(ConsoleColor.Yellow)} done" : $"{(progress.ToString() + "%").Color(Color.LawnGreen)} done in {DateTime.Now - Holder.ProcessTime}");
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

string GetVersionString(Assembly assembly)
{
    var ver = new Version(assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
    return string.Join('.', ver.Major, ver.Minor, ver.Build);
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

        Console.WriteLine($"Generating keystore with {keystoreSize.ToString().Color(ConsoleColor.Cyan)} keys");
        ks = KeyStore.Generate(keystoreSize);
    }

    Console.WriteLine($"Keystore Fingerprint: {ks.Fingerprint.FormatFingerprint().Color(Color.LightSkyBlue)}");

    var fName = !string.IsNullOrEmpty(name) && !useToken ? name : BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";
#if DEBUG
    File.WriteAllText(fName, Convert.ToBase64String(ks.Raw));
    Console.WriteLine($"Keystore is saved to {fName.Color(ConsoleColor.Green)} as base64");
#else
    File.WriteAllBytes(fName, ks.Raw);
    Console.WriteLine($"Keystore is saved to {fName.Color(ConsoleColor.Green)}");
#endif

    return ks;
}

Kes InitKES(Arguments opt, Header header)
{
    Kes kp = new(header);
    kp.OnProgress += ShowProgress;
    return kp;
}