using System.Reflection;
using System.Text;

using SAPTeam.CommonTK.Console;
using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Tool;

Arguments opt = GetArguments();
IsValid();

string appVer = GetVersionString(Assembly.GetAssembly(typeof(Program)));
Echo(new Colorize($"Kryptor Command-Line Interface v[{appVer}]", ConsoleColor.DarkCyan));

string engVer = GetVersionString(Assembly.GetAssembly(typeof(KES)));
Echo(new Colorize($"Engine version: [{engVer}]", ConsoleColor.DarkGreen));

if (opt.Encrypt || opt.Decrypt)
{
    KeyStore ks = default;
    KES kp = InitKES(opt);

    if (opt.Decrypt || !opt.CreateKey)
    {
        ks = ReadKeystore(opt.KeyStore);
        StandaloneKeyCryptoProvider skcp = new StandaloneKeyCryptoProvider(ks, opt.Continuous);
        kp.Provider = skcp;
    }

    if (opt.Encrypt)
    {
        foreach (var file in opt.File)
        {
            if (opt.CreateKey)
            {
                ks = GenerateKeystore();
                StandaloneKeyCryptoProvider skcp = new StandaloneKeyCryptoProvider(ks, opt.Continuous);
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

async Task Encrypt(string file, KES kp)
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

    var header = new Header()
    {
        DetailLevel = HeaderDetails.Normal,
        OriginalName = Path.GetFileName(file)
    };

    await kp.EncryptAsync(f, f2);

    Echo(new Colorize($"Saved to [{resolvedName}]", ConsoleColor.Green));
}

async Task Decrypt(string file, KES kp, string ksFingerprint)
{
    Echo(new Colorize($"Decrypting [{Path.GetFileName(file)}]", ConsoleColor.Cyan));

    if (!CheckFile(file))
    {
        return;
    }

    try
    {
        using var f = File.OpenRead(file);

        var header = Header.ReadHeader(f);

        if (header.Version == null)
        {
            Echo(new Colorize("[Warning:] Invalid header found, The decryption may be fail", ConsoleColor.Yellow));
        }

        if (header.Fingerprint != null)
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

        if (header.Version != null && header.Version != KES.Version)
        {
            if (header.EngineVersion != null)
            {
                Echo(new Colorize($"[Failed:] Encryptor api version is not supported. You must use kryptor v{header.EngineVersion}", ConsoleColor.Red));
                Environment.ExitCode = 0xFE;
                return;
            }
            else
            {
                Echo(new Colorize("[Failed:] Encryptor api version is not supported.", ConsoleColor.Red));
                Environment.ExitCode = 0xFE;
                return;
            }
        }

        string origName = header.OriginalName;
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
    bool useGenerica = keystore.StartsWith("generica:");

    if (!File.Exists(keystore) && !useGenerica)
    {
        Echo(new Colorize("[Error:] Keystore not found.", ConsoleColor.Red));
        Environment.Exit(3);
        return default;
    }

    KeyStore ks;

    try
    {
        if (useGenerica)
        {
            string gSeed = keystore.Split(':')[1];
            int gSize = int.Parse(keystore.Split(':')[2]);
            Echo(new Colorize($"Generica seed: [{gSeed}]:[{gSize}]", ConsoleColor.DarkYellow));
            Generica gen = new Generica(Encoding.UTF8.GetBytes(gSeed));
            byte[] buffer = new byte[gSize * 32];
            gen.Generate(buffer);
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
            if (opt.File != null)
            {
                opt.File = new string[] { opt.KeyStore }.Concat(opt.File);
            }
            else
            {
                opt.File = new string[] { opt.KeyStore };
            }
        }

        if (opt.File == null || !opt.File.Any())
        {
            Echo(new Colorize("[Error:] You must specify at least one file.", ConsoleColor.Red));
            Environment.Exit(2);
        }

        if (!KES.ValidateBlockSize(opt.BlockSize))
        {
            Echo(new Colorize("[Error:] Block size must be multiple of 32.", ConsoleColor.Red));
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
    bool useGenerica = false;

    if (keystoreSize == 0)
    {
        keystoreSize = KeyStore.GetRandomOddNumber();
    }

    Echo(new Colorize($"Generating keystore with [{keystoreSize}] keys", ConsoleColor.Cyan), false);

    if (name.StartsWith("generica:"))
    {
        useGenerica = true;
        Echo(" using Generica");
        Generica gen = new Generica(Encoding.UTF8.GetBytes(name.Split(':')[1]));
        byte[] buffer = new byte[keystoreSize * 32];
        gen.Generate(buffer);
        ks = new KeyStore(buffer);
    }
    else
    {
        ks = KeyStore.Generate(keystoreSize);
    }

    Echo(new Colorize($"Keystore Fingerprint: [{ks.Fingerprint.FormatFingerprint()}]", ConsoleColor.Blue));

    var fName = !string.IsNullOrEmpty(name) && !useGenerica ? name : BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";
    File.WriteAllBytes(fName, ks.Raw);

    Echo(new Colorize($"Keystore is saved to [{fName}]", ConsoleColor.Green));
    return ks;
}

KES InitKES(Arguments opt)
{
    KES kp = new(maxBlockSize: opt.BlockSize);
    kp.OnProgress += ShowProgress;
    return kp;
}