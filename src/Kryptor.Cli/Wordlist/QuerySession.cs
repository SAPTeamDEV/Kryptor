using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class QuerySession : Session
    {
        private readonly WordlistIndexEntry Entry;
        private readonly string Word;

        public bool Result = false;

        public QuerySession(WordlistIndexEntry entry, string word)
        {
            Entry = entry;
            Word = word;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Progress = -1;
            Description = $"Querying in {Entry.Id}";

            WordlistFragmentCollection wfc = Entry.Open();
            bool result = await wfc.LookupAsync(Word, cancellationToken);

            Result = result;
            Description += " : " + (Result ? "Found" : "Not found");
            return true;
        }
    }
}