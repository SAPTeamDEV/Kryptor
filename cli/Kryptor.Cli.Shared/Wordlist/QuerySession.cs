using System.Threading;
using System.Threading.Tasks;

#if !NETFRAMEWORK
using ANSIConsole;
#endif

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class QuerySession : Session
    {
        private readonly WordlistIndexEntryV2 Entry;
        private readonly string Word;

        public bool Result = false;

        public QuerySession(WordlistIndexEntryV2 entry, string word)
        {
            Entry = entry;
            Word = word;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Progress = -1;
            Description = $"Querying in {Entry.Id}";

            var wfc = new WordlistFragmentCollection(Entry.InstallDirectory);
            bool result = await wfc.LookupAsync(Word, cancellationToken);

            Result = result;
            Description += " : " + (Result ? "Found" : "Not found");
            return true;
        }
    }
}