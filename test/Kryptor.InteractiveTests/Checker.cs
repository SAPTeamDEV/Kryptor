internal class Checker
{
    /*
    public static void Main()
    {
        int mult = 1;

        int? lValue = null;
        int? lStep = null;

        while (true)
        {
            string text = "a";
            string mText = "";

            for (int i = 0; i < mult; i++)
            {
                mText += text;
            }

            string b64 = Convert.ToBase64String(mText);
            string key = new string('k', 32);

            string cipher = EncryptProvider.AESEncrypt(b64, key);
            int diff = cipher.Length - mText.Length;

            Console.WriteLine($"Step: {mult}, Input Size: {b64.Length}, Cipher Length: {cipher.Length}, Diff: {diff}");

            if (lValue == null || lValue >= diff)
            {
                lValue = diff;
                lStep = mult;
            }

            if (mult == 500) break;
            mult++;
        }

        Console.WriteLine($"The best choice is: {lStep} whith {lValue} diff");
    }

    public static void CheckCunckSize()
    {
        string key = KeyStore.Generate().Keys.First();

        int chunkSize = 1;

        int? lChunk = null;
        int? lValue = null;
        byte[] data = File.ReadAllBytes(@"D:\Desktop\test.txt");

        while (true)
        {
            var chunk = data.Slice<byte>(chunkSize).First();
            var b64 = Convert.ToBase64String(chunk);
            var b = EncryptProvider.AESEncrypt(b64, key);
            int diff = b.Length - b64.Length;

            Console.WriteLine($"Chunck Size: {chunkSize}, b64 Length: {b64.Length}, Encrypted Size: {b.Length}, diff: {diff}");

            if (lValue == null || lValue >= diff)
            {
                lValue = diff;
                lChunk = chunkSize;
            }

            if (chunkSize == 972) break;
            chunkSize++;
        }

        Console.WriteLine($"The best choice is: {lChunk} whith {lValue} diff");

    }

    public static void CheckSize(string[] args)
    {
        KESProvider kp = new(KeyStore.Generate());

        int chunkSize = 1;

        int? lChunk = null;
        int? lValue = null;
        byte[] data = File.ReadAllBytes(args[0]);

        while (true)
        {
            var enc = kp.Encrypt(data, chunkSize);
            int diff = enc.Length - data.Length;

            Console.WriteLine($"Chunk Size: {chunkSize}, Data Size: {data.Length}, Cipher Length: {enc.Length}, diff: {diff}");

            if (lValue == null || lValue > diff)
            {
                lValue = diff;
                lChunk = chunkSize;
            }

            if (chunkSize == 972) break;
            chunkSize++;
        }

        Console.WriteLine($"The best choice is: {lChunk} whith {lValue} diff");
    }
    */
}