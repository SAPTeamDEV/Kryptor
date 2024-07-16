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

var con = new CryptoProviderConfiguration()
{
    Id = pIn,
};

var inHeader = new Header()
{
    Verbosity = HeaderVerbosity.Normal,
};

if (!File.Exists("test.kks"))
{
    File.WriteAllBytes("test.kks", KeyStore.Generate().Raw);
}

KeyStore ks = new KeyStore(File.ReadAllBytes("test.kks"));
CryptoProvider cp = CryptoProviderFactory.Create(ks, con);
Kes kp = new(cp, inHeader.BlockSize);

while (true)
{
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
        Console.WriteLine($"Verbosity: {header.Verbosity}");
        if (header.Version != null)
        {
            Console.WriteLine($"API Version: {header.Version}");
        }

        if (header.EngineVersion != null)
        {
            Console.WriteLine($"Engine Version: {header.EngineVersion}");
        }

        if (header.BlockSize > 0)
        {
            Console.WriteLine($"Block Size: {header.BlockSize}");
        }

        if (header.Configuration != null)
        {
            Console.WriteLine($"Id: {header.Configuration.Id}");
            Console.WriteLine($"Continuous: {header.Configuration.Continuous}");
            Console.WriteLine($"Remove Hash: {header.Configuration.RemoveHash}");
            Console.WriteLine($"Dynamic Block Processing: {header.Configuration.DynamicBlockProccessing}");
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