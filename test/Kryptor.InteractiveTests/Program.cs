using System.Text;

using MoreLinq;

using SAPTeam.Kryptor;

/*
Checker.CheckSize(args);
return;
*/

string text;
string? input = null;
int mult = 0;

while (true)
{
    if (!File.Exists("test.kks"))
    {
        File.WriteAllBytes("test.kks", KeyStore.Generate().Raw);
    }

    Kes kp = new(new StandaloneKeyCryptoProvider(new KeyStore(File.ReadAllBytes("test.kks"))));

    bool printlog = args.Length < 1;
    
    Console.Write("Enter text");

    if (!string.IsNullOrEmpty(input))
    {
        Console.Write($" ({input})");
    }

    Console.Write(": ");

    string? tIn = Console.ReadLine();
    
    if (!string.IsNullOrEmpty(tIn))
    {
        input = tIn;
    }

    Console.Write("Enter multiplier");

    if (mult > 0)
    {
        Console.Write($" ({mult})");
    }

    Console.Write(": ");

    try
    {
        string? tMul = Console.ReadLine();

        if (!string.IsNullOrEmpty(tMul))
        {
            mult = int.Parse(tMul);
        }
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

    Console.WriteLine();
}