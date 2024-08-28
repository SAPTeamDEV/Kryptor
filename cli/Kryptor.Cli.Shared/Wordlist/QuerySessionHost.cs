using System;
using System.Linq;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class QuerySessionHost : SessionHost
    {
        private readonly string Query;
        private readonly string Wordlist;

        public QuerySessionHost(GlobalOptions globalOptions, string query, string wordlist) : base(globalOptions)
        {
            Query = query;
            Wordlist = wordlist;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

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
                foreach (Client.Security.WordlistIndexEntry entry in LocalIndex.Wordlists)
                {
                    QuerySession session = new QuerySession(entry, Query);
                    NewSession(session);
                }
            }
            else
            {
                QuerySession session = new QuerySession(LocalIndex[Wordlist], Query);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();

            System.Collections.Generic.IEnumerable<QuerySession> ranQueries = Container.Sessions.OfType<QuerySession>();
            if (!ranQueries.Any()) return;

            Log();
            Log("Query summary");

            int found = 0;
            int notFound = 0;
            int failed = 0;

            foreach (QuerySession s in ranQueries)
            {
                if (s.EndReason == SessionEndReason.Completed)
                {
                    if (s.Result)
                    {
                        found++;
                    }
                    else
                    {
                        notFound++;
                    }
                }
                else
                {
                    failed++;
                }
            }

            Log($"Found: {found}");
            Log($"Not Found: {notFound}");
            Log($"Failed: {failed}");
            Log($"Total: {ranQueries.Count()}");
        }
    }
}