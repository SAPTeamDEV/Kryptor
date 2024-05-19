using System.Text;

using MoreLinq;

using SAPTeam.Kryptor;

/*
Checker.CheckSize(args);
return;
*/

while (true)
{
    if (!File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "testkey")))
    {
        File.WriteAllBytes(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "testkey"), KeyStore.Generate().Raw);
    }

    Kes2 kp = new(new StandaloneKeyCryptoProvider(new KeyStore(File.ReadAllBytes(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "testkey")))));

    bool printlog = args.Length < 1;

    if (args.Length == 1)
    {
        /*
        if (args[0].EndsWith(".enc"))
        {
            kp.DecryptFile(args[0], args[0] + ".orig");
        }
        else
        {
            kp.EncryptFile(args[0], args[0] + ".enc");
        }
        */
        Console.WriteLine("success");
        Console.ReadKey();
        return 0;
    }
    else
    {
        string text;
        Console.Write("Enter text: ");
        var input = Console.ReadLine();

        Console.Write("Enter multiplier: ");
        int mult;

        try
        {
            mult = int.Parse(Console.ReadLine());
        }
        catch (Exception)
        {
            mult = 1;
        }

        text = string.Concat(input.Repeat(mult));

        var enc = kp.EncryptBlockAsync(Encoding.UTF8.GetBytes(text)).Result;
        Console.WriteLine($"Text Size: {text.Length}, Cipher Length: {enc.Length}");
        if (printlog)
        {
            Console.WriteLine(Convert.ToBase64String(enc));
            Console.WriteLine();
        }

        var dec = Encoding.UTF8.GetString(kp.DecryptBlockAsync(enc).Result);
        if (printlog)
        {
            Console.WriteLine(dec);
        }
        else
        {
            Console.WriteLine("Success");
        }
    }

    Console.WriteLine();
}