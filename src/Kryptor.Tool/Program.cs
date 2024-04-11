using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Tool;

var result = Parser.Default.ParseArguments<Arguments>(args);
if (result == null || result.Value == null)
{
    return 0x1;
}

Console.WriteLine("Kryptor Command-Line Interface");
Arguments opt = result.Value;

if (opt.Encrypt)
{
    Console.WriteLine($"Reading keystore: {Path.GetFileName(opt.KeyStore)}");
    KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText(opt.KeyStore));
    Console.WriteLine($"Keystore Fingerprint: {BitConverter.ToString(ks.Fingerprint)}");
    KESProvider kp = new(ks);
    kp.OnProgress += Utils.ShowProgress;

    foreach (var file in opt.File)
    {
        Console.WriteLine($"Encrypting {Path.GetFileName(file)}");
        await kp.EncryptFileAsync(file, file + ".kef");
    }
}
else if (opt.Decrypt)
{
    Console.WriteLine($"Reading keystore: {Path.GetFileName(opt.KeyStore)}");
    KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText(opt.KeyStore));
    Console.WriteLine($"Keystore Fingerprint: {BitConverter.ToString(ks.Fingerprint)}");
    KESProvider kp = new(ks);
    kp.OnProgress += Utils.ShowProgress;

    foreach (var file in opt.File)
    {
        Console.WriteLine($"Decrypting {Path.GetFileName(file)}");
        await kp.DecryptFileAsync(file, file + ".decrypted");
    }
}
else if (opt.Generate)
{
    Console.WriteLine($"Generating keystore with {opt.KeyStoreSize} keys");
    KESKeyStore ks = KESKeyStore.Generate(opt.KeyStoreSize);
    Console.WriteLine($"Keystore Fingerprint: {BitConverter.ToString(ks.Fingerprint)}");
    var fName = !string.IsNullOrEmpty(opt.KeyStore) ? opt.KeyStore : BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";
    File.WriteAllText(fName, ks.ToString());
    Console.WriteLine($"Keystore is saved to {fName}");
}

return 0x0;