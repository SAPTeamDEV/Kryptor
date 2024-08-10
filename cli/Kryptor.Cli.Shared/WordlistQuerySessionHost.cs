using System;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistQuerySessionHost : WordlistSessionHost
    {
        string Query;
        string Wordlist;

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
                foreach (var entry in LocalIndex.Wordlists)
                {
                    var session = new WordlistQuerySession(entry, Query);
                    NewSession(session);
                }
            }
            else
            {
                var session = new WordlistQuerySession(LocalIndex[Wordlist], Query);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}