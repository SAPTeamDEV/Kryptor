using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if !NETFRAMEWORK
using ANSIConsole;
#endif

using SAPTeam.Kryptor.Cli;
using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistQuerySession : Session
    {
        WordlistIndexEntryV2 Entry;
        string Word;

        public bool Result = false;

        public WordlistQuerySession(WordlistIndexEntryV2 entry, string word)
        {
            Entry = entry;
            Word = word;
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            Progress = -1;
            Description = $"Querying in {Entry.Id}";

            var wl = new Wordlist(Entry.InstallDirectory);
            var result = await wl.ContainsAsync(Word, cancellationToken);

            Result = result;
            Description += " : " + (Result ? "Found" : "Not found");
            return true;
        }
    }
}