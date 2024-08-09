using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistQuerySession : Session
    {
        string Id;
        WordlistIndexEntry Entry;
        string Word;

        public bool Result = false;

        public WordlistQuerySession(string id, WordlistIndexEntry entry, string word)
        {
            Id = id;
            Entry = entry;
            Word = word;
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            Progress = -1;
            Description = $"Querying in {Id}";

            var wl = new Wordlist(Entry.InstallDirectory);
            var result = await wl.ContainsAsync(Word, cancellationToken);

            Result = result;
            Description += " : " + (Result ? "Found" : "Not found");
            return true;
        }
    }
}