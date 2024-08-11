using System;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistQuerySessionHost : WordlistSessionHost
    {
        private readonly string Query;
        private readonly string Wordlist;

        public WordlistQuerySessionHost(bool verbose, string query, string wordlist) : base(verbose)
        {
            Query = query;
            Wordlist = wordlist;
        }

        public override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Query))
            {
                throw new ArgumentNullException("Word");
            }

            if (Query.Length < 4)
            {
                throw new ArgumentException("The given word is smaller than 4 characters");
            }

            if (string.IsNullOrEmpty(Wordlist))
            {
                foreach (Client.Security.WordlistIndexEntryV2 entry in LocalIndex.Wordlists)
                {
                    WordlistQuerySession session = new WordlistQuerySession(entry, Query);
                    NewSession(session);
                }
            }
            else
            {
                WordlistQuerySession session = new WordlistQuerySession(LocalIndex[Wordlist], Query);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}