using System.Text;

using SAPTeam.Kryptor.Security;

public class Program
{
    static Dictionary<string, FileStream> fileStreams = new Dictionary<string, FileStream>();

    public static void Main(string[] args)
    {
        string fileName = args[0];

        Console.WriteLine("Please wait");

        foreach (var line in File.ReadAllLines(fileName))
        {
            if (string.IsNullOrEmpty(line) || line.Length < 4) continue;
            string c = Wordlist.GetWordIdentifier(line).ToString();
            if (!fileStreams.ContainsKey(c))
            {
                fileStreams[c] = File.OpenWrite(c + ".txt");
            }
            var data = Encoding.UTF8.GetBytes(line + "\n");

            fileStreams[c].Write(data, 0, data.Length);
        }

        foreach (var f in fileStreams.Values)
        {
            f.Flush();
            f.Dispose();
        }

        Console.WriteLine("Done.");
    }
}