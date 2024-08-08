using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistQuerySession : Session
    {
        string path;
        string word;

        public bool Result = false;

        public WordlistQuerySession(string wordlistPath, string word)
        {
            path = wordlistPath;
            this.word = word;
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            Progress = -1;
            Description = $"Querying in {Path.GetFileName(path)}";

            var wl = new Wordlist(path);
            var result = await wl.ContainsAsync(word, cancellationToken);

            Result = result;
            Description += " : " + (Result ? "Found" : "Not found");
            return true;
        }
    }
}