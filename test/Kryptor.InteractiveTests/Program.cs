using System.Text;

using MoreLinq;

using SAPTeam.Kryptor;

/*
Checker.CheckSize(args);
return;
*/

bool printlog = args.Length < 1;
bool echoTexts = printlog;
string text;
string? input = null;
int mult = 0;

Console.Write("Enter CryptoProvider numeric ID: ");
var pIn = Console.ReadLine();
CryptoTypes ct = (CryptoTypes)int.Parse(pIn);

var inHeader = new Header()
{
    CryptoType = ct,
    DetailLevel = HeaderDetails.Normal,
};

KeyStore ks = new KeyStore(File.ReadAllBytes("test.kks"));
CryptoProvider cp = CryptoProviderFactory.Create(ks, inHeader);
Kes kp = new(cp, inHeader);

while (true)
{
    if (!File.Exists("test.kks"))
    {
        File.WriteAllBytes("test.kks", KeyStore.Generate().Raw);
    }

    Console.Write("Enter text");

    if (!string.IsNullOrEmpty(input))
    {
        Console.Write($" ({input})");
    }

    Console.Write(": ");

    string? tIn = Console.ReadLine();

    if (!string.IsNullOrEmpty(tIn))
    {
        if (tIn == "-s")
        {
            echoTexts = false;
            continue;
        }

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

    var source = new MemoryStream(Encoding.UTF8.GetBytes(text));
    var dest = new MemoryStream();

    await kp.EncryptAsync(source, dest);

    Console.WriteLine($"Text Size: {text.Length}, Cipher Length: {dest.Length}");
    if (printlog && echoTexts)
    {
        Console.WriteLine(Convert.ToBase64String(dest.ToArray()));
        Console.WriteLine();
    }

    Header header = Header.ReadHeader<Header>(dest);
    if (printlog)
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

        if (header.Extra != null)
        {
            Console.WriteLine($"Extra data:\n{string.Join(Environment.NewLine, header.Extra)}");
        }
    }

    var decStream = new MemoryStream();
    await kp.DecryptAsync(dest, decStream);

    if (printlog && echoTexts)
    {
        Console.WriteLine(Encoding.UTF8.GetString(decStream.ToArray()));
    }
    else
    {
        Console.WriteLine("Success");
    }

    Console.WriteLine();
}