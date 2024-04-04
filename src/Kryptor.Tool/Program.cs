using SAPTeam.Kryptor;
using SAPTeam.Kryptor.Tool;

Console.WriteLine("Kryptor Command-Line Interface");

Parser.Default.ParseArguments<Arguments>(args).WithParsed(async opt =>
{
    if (opt.Encrypt)
    {
        Console.WriteLine($"Reading keystore: {Path.GetFileName(opt.KeyStore)}");
        KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText(opt.KeyStore));
        KESProvider kp = new(ks);
        foreach (var file in opt.File)
        {
            Console.WriteLine($"Encrypting {Path.GetFileName(file)}");
            await kp.EncryptFileAsync(file, file + ".kef");
        }
    } else if (opt.Decrypt)
    {
        Console.WriteLine($"Reading keystore: {Path.GetFileName(opt.KeyStore)}");
        KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText(opt.KeyStore));
        KESProvider kp = new(ks);
        foreach (var file in opt.File)
        {
            Console.WriteLine($"Decrypting {Path.GetFileName(file)}");
            await kp.DecryptFileAsync(file, file + ".decrypted");
        }
    } else if (opt.Generate)
    {
        Console.WriteLine($"Generating keystore with {opt.KeyStoreSize} keys");
        KESKeyStore ks = KESKeyStore.Generate(opt.KeyStoreSize);
        File.WriteAllText(opt.KeyStore, ks.ToString());
        Console.WriteLine($"Keystore is saved to {opt.KeyStore}");
    }
});