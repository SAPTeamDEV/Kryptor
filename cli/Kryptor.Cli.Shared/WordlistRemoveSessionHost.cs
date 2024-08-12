using System.Linq;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistRemoveSessionHost : WordlistSessionHost
    {
        private readonly bool List;
        private readonly bool All;
        private string[] Wordlists;

        public WordlistRemoveSessionHost(bool verbose, bool list, bool all, string[] wordlists) : base(verbose)
        {
            List = list;
            All = all;
            Wordlists = wordlists;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (List || (!All && Wordlists.Length == 0))
            {
                ListInstalledWordlists();
                return;
            }

            if (All)
            {
                Wordlists = LocalIndex.Wordlists.Select(x => x.Id).ToArray();
                if (Wordlists.Length == 0)
                {
                    Log("There is no installed wordlist to remove");
                    return;
                }
            }
            else
            {
                Wordlists = Wordlists.Where(LocalIndex.ContainsId).ToArray();
                if (Wordlists.Length == 0)
                {
                    Log("No valid id are supplied");
                }
            }

            if (Wordlists.Length > 0)
            {
                foreach (string id in Wordlists)
                {
                    Log($"Removing {id}");
                    RemoveWordlist(id);
                }
            }
        }
    }
}