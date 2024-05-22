using System.Reflection;

using SAPTeam.CommonTK.Console;
using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Cli;

Arguments opt = GetArguments();
IsValid();

string appVer = GetVersionString(Assembly.GetAssembly(typeof(Program)));
Echo(new Colorize($"Kryptor Command-Line Interface v[{appVer}]", ConsoleColor.DarkCyan));

string engVer = GetVersionString(Assembly.GetAssembly(typeof(Kes)));
Echo(new Colorize($"Engine version: [{engVer}]", ConsoleColor.DarkGreen));

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
    Echo(new Colorize($"Encrypting [{Path.GetFileName(file)}]", ConsoleColor.Cyan));

    if (!CheckFile(file))
    {
        return;
    }

    using var f = File.OpenRead(file);
    string resolvedName = GetNewFileName(file, file + ".kef");

    Echo("Prepairing");
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

    Echo(new Colorize($"Saved to [{resolvedName}]", ConsoleColor.Green));
}

async Task Decrypt(string file, Kes kp, string ksFingerprint)
{
    Echo(new Colorize($"Decrypting [{Path.GetFileName(file)}]", ConsoleColor.Cyan));

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
            Echo(new Colorize("[Warning:] Empty header found, The decryption may be fail", ConsoleColor.Yellow));
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
                Echo(new Colorize($"[Failed:] Encryptor api version is not supported. You must use kryptor cli v{header.CliVersion}", ConsoleColor.Red));
                Environment.ExitCode = 0xFE;
                return;
            }
            else
            {
                Echo(new Colorize($"[Failed:] Encryptor api version is not supported. You must use kryptor v{header.EngineVersion}", ConsoleColor.Red));
                Environment.ExitCode = 0xFE;
                return;
            }
        }

        if ((int)header.DetailLevel > 1)
        {
            string fingerprint = header.Fingerprint.FormatFingerprint();

            Echo(new Colorize($"File Fingerprint: [{fingerprint}]", ConsoleColor.DarkRed));

            if (fingerprint != ksFingerprint)
            {
                Echo(new Colorize("[Failed:] Fingerprints does not match.", ConsoleColor.Red));
                Environment.ExitCode = 0xFE;
                return;
            }
        }

        string origName = header.OriginalName ?? "decrypted file.dec";
        string resolvedName = GetNewFileName(file, origName);

        Echo("Prepairing");
        using var f2 = File.OpenWrite(resolvedName);

        await kp.DecryptAsync(f, f2);

        Echo(new Colorize($"Saved to [{resolvedName}]", ConsoleColor.Green));
    }
    catch (InvalidDataException)
    {
        Echo(new Colorize("[Failed:] Cannot decrypt file.", ConsoleColor.Red));
        Environment.ExitCode = 0xFF;
    }
    catch (ArgumentException)
    {
        Echo(new Colorize("[Failed:] Cannot decrypt file, File maybe corrupted!", ConsoleColor.Red));
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
        Echo(new Colorize("[Skipped:] File not found", ConsoleColor.DarkGray));
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
        Echo(new Colorize("[Error:] Keystore not found.", ConsoleColor.Red));
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
            Echo(new Colorize($"Reading keystore: [{Path.GetFileName(keystore)}]", ConsoleColor.DarkYellow));
            ks = new KeyStore(File.ReadAllBytes(keystore));
        }

        Echo(new Colorize($"Keystore Fingerprint: [{ks.Fingerprint.FormatFingerprint()}]", ConsoleColor.Blue));
        return ks;
    }
    catch (FormatException)
    {
        Echo(new Colorize("[Error:] Cannot read keystore.", ConsoleColor.Red));
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
                Echo(new Colorize("[Error:] You must specify keysore file name.", ConsoleColor.Red));
                Environment.Exit(2);
            }
        }
        else if (!string.IsNullOrEmpty(opt.KeyStore))
        {
            opt.File = opt.File != null ? new string[] { opt.KeyStore }.Concat(opt.File) : (IEnumerable<string>)(new string[] { opt.KeyStore });
        }

        if (opt.File == null || !opt.File.Any())
        {
            Echo(new Colorize("[Error:] You must specify at least one file.", ConsoleColor.Red));
            Environment.Exit(2);
        }
    }
}

void ShowProgress(int progress)
{
    ClearLine();
    Echo(new Colorize(progress < 100 ? $"[{progress}%] done" : $"[{progress}%] done in {DateTime.Now - Holder.ProcessTime}", progress < 100 ? ConsoleColor.Yellow : ConsoleColor.Green));
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
            Echo(new Colorize("[Eroor:] Invalid transformer token", ConsoleColor.Red));
            Environment.Exit(2);
        }

        if (keystoreSize == 0)
        {
            keystoreSize = KeyStore.GetRandomOddNumber();
        }

        Echo(new Colorize($"Generating keystore with [{keystoreSize}] keys", ConsoleColor.Cyan));
        ks = KeyStore.Generate(keystoreSize);
    }

    Echo(new Colorize($"Keystore Fingerprint: [{ks.Fingerprint.FormatFingerprint()}]", ConsoleColor.Blue));

    var fName = !string.IsNullOrEmpty(name) && !useToken ? name : BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";
#if DEBUG
    File.WriteAllText(fName, Convert.ToBase64String(ks.Raw));
    Echo(new Colorize($"Keystore is saved to [{fName}] as base64", ConsoleColor.Green));
#else
    File.WriteAllBytes(fName, ks.Raw);
    Echo(new Colorize($"Keystore is saved to [{fName}]", ConsoleColor.Green));
#endif

    return ks;
}

Kes InitKES(Arguments opt, Header header)
{
    Kes kp = new(header);
    kp.OnProgress += ShowProgress;
    return kp;
}