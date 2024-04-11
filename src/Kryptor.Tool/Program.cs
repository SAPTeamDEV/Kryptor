using System.Reflection;

using SAPTeam.CommonTK.Console;
using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Tool;

var result = Parser.Default.ParseArguments<Arguments>(args);
if (result == null || result.Value == null)
{
    return 0x1;
}

var ver = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
string verStr = string.Join('.', ver.Major, ver.Minor, ver.Build);

var engVer = new Version(Assembly.GetAssembly(typeof(KESProvider)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
string engVerStr = string.Join('.', engVer.Major, engVer.Minor, engVer.Build);

Echo(new Colorize($"Kryptor Command-Line Interface v[{verStr}]", ConsoleColor.DarkCyan));
Echo(new Colorize($"Engine version: [{engVerStr}]", ConsoleColor.DarkGreen));
Arguments opt = result.Value;

if (opt.Encrypt)
{
    KESKeyStore ks = ReadKeystore(opt);
    KESProvider kp = new(ks);
    kp.OnProgress += Helper.ShowProgress;

    foreach (var file in opt.File)
    {
        Echo(new Colorize($"Encrypting [{Path.GetFileName(file)}]", ConsoleColor.Green));
        await kp.EncryptFileAsync(file, file + ".kef");
    }
}
else if (opt.Decrypt)
{
    KESKeyStore ks = ReadKeystore(opt);
    KESProvider kp = new(ks);
    kp.OnProgress += Helper.ShowProgress;

    foreach (var file in opt.File)
    {
        Echo(new Colorize($"Decrypting [{Path.GetFileName(file)}]", ConsoleColor.Green));
        await kp.DecryptFileAsync(file, file + ".decrypted");
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

return 0x0;

static KESKeyStore ReadKeystore(Arguments opt)
{
    Echo(new Colorize($"Reading keystore: [{Path.GetFileName(opt.KeyStore)}]", ConsoleColor.DarkYellow));
    KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText(opt.KeyStore));
    Echo(new Colorize($"Keystore Fingerprint: [{BitConverter.ToString(ks.Fingerprint)}]", ConsoleColor.Blue));
    return ks;
}