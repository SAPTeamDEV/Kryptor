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

            if (string.IsNullOrEmpty(Wordlist))
            {
                foreach (var wl in LocalIndex.Wordlists)
                {
                    var session = new WordlistQuerySession(wl.Key, wl.Value, Query);
                    NewSession(session);
                }
            }
            else
            {
                var session = new WordlistQuerySession(Wordlist, LocalIndex[Wordlist], Query);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();
        }
    }
}