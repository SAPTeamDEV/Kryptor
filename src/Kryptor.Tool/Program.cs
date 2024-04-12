using System.Reflection;

using SAPTeam.CommonTK.Console;
using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Tool;

Arguments opt = GetArguments(args);

string appVer = Helper.GetVersionString(Assembly.GetAssembly(typeof(Program)));
Echo(new Colorize($"Kryptor Command-Line Interface v[{appVer}]", ConsoleColor.DarkCyan));

string engVer = Helper.GetVersionString(Assembly.GetAssembly(typeof(KESProvider)));
Echo(new Colorize($"Engine version: [{engVer}]", ConsoleColor.DarkGreen));

if (opt.Encrypt || opt.Decrypt)
{
    IsValid(opt);

    KESKeyStore ks = ReadKeystore(opt);

    KESProvider kp = new(ks, continuous: opt.Continuous);
    kp.OnProgress += Helper.ShowProgress;

    if (opt.Encrypt)
    {
        foreach (var file in opt.File)
        {
            await Encrypt(file, kp);
        }
    }
    else
    {
        foreach (var file in opt.File)
        {
            await Decrypt(file, kp, BitConverter.ToString(ks.Fingerprint));
        }
    }
}
else if (opt.Generate)
{
    Echo(new Colorize($"Generating keystore with [{opt.KeyStoreSize}] keys", ConsoleColor.Cyan));
    KESKeyStore ks = KESKeyStore.Generate(opt.KeyStoreSize);

    Echo(new Colorize($"Keystore Fingerprint: [{BitConverter.ToString(ks.Fingerprint)}]", ConsoleColor.Blue));

    var fName = !string.IsNullOrEmpty(opt.KeyStore) ? opt.KeyStore : BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";
    File.WriteAllText(fName, ks.ToString());

    Echo(new Colorize($"Keystore is saved to [{fName}]", ConsoleColor.Green));
}

return Environment.ExitCode;

async Task Encrypt(string file, KESProvider kp)
{
    Echo(new Colorize($"Encrypting [{Path.GetFileName(file)}]", ConsoleColor.Green));

    using var f = File.OpenRead(file);
    string resolvedName = Helper.GetNewFileName(file, file + ".kef");

    Echo("Openning file stream");
    using var f2 = File.OpenWrite(resolvedName);

    await kp.EncryptFileAsync(f, f2);

    ClearLine(true);
    Echo(new Colorize($"Saved to [{resolvedName}]", ConsoleColor.Green));
}

async Task Decrypt(string file, KESProvider kp, string ksFingerprint)
{
    Echo(new Colorize($"Decrypting [{Path.GetFileName(file)}]", ConsoleColor.Green));

    try
    {
        using var f = File.OpenRead(file);

        var header = KESProvider.ReadHeader(f);
        string fingerprint = BitConverter.ToString(header.fingerprint);

        Echo(new Colorize($"File Fingerprint: [{fingerprint}]", ConsoleColor.DarkRed));

        if (fingerprint != ksFingerprint)
        {
            Echo(new Colorize("[Failed:] Fingerprints does not match.", ConsoleColor.Red));
            Environment.ExitCode = 0xFE;
            return;
        }

        string origName = header.fileName;
        string resolvedName = Helper.GetNewFileName(file, origName);

        Echo("Openning file stream");
        using var f2 = File.OpenWrite(resolvedName);

        await kp.DecryptFileAsync(f, f2);

        ClearLine();
        Echo(new Colorize($"Saved to [{resolvedName}]", ConsoleColor.Green));
    }
    catch (InvalidDataException)
    {
        Echo(new Colorize("[Failed:] Cannot decrypt file.", ConsoleColor.Red));
        Environment.ExitCode = 0xFF;
    }
}

Arguments GetArguments(string[] args)
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

KESKeyStore ReadKeystore(Arguments opt)
{
    try
    {
        Echo(new Colorize($"Reading keystore: [{Path.GetFileName(opt.KeyStore)}]", ConsoleColor.DarkYellow));
        KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText(opt.KeyStore));

        Echo(new Colorize($"Keystore Fingerprint: [{BitConverter.ToString(ks.Fingerprint)}]", ConsoleColor.Blue));
        return ks;
    }
    catch (FormatException)
    {
        Echo(new Colorize("[Error:] Cannot read keystore.", ConsoleColor.Red));
        Environment.Exit(3);
        return default;
    }
}

void IsValid(Arguments opt)
{
    if (string.IsNullOrEmpty(opt.KeyStore) || opt.File.Count() == 0)
    {
        Echo(new Colorize("[Error:] You must specify keystore and at least one file.", ConsoleColor.Red));
        Environment.Exit(2);
    }
}
