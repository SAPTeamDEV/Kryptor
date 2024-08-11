using System.Linq;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistRemoveSessionHost : WordlistSessionHost
    {
        bool List;
        bool All;
        string[] Wordlists;

        public WordlistRemoveSessionHost(bool verbose, bool list, bool all, string[] wordlists) : base(verbose)
        {
            List = list;
            All = all;
            Wordlists = wordlists;
        }

        public override void Start()
        {
            base.Start();

            if (List || (!All && !Wordlists.Any()))
            {
                ListInstalledWordlists();
                return;
            }

            if (All)
            {
                Wordlists = LocalIndex.Wordlists.Select(x => x.Id).ToArray();
                if (Wordlists.Length == 0)
                {
                    Log("There is no installed wordlist");
                    return;
                }
            }
            else
            {
                Wordlists = Wordlists.Where(x => LocalIndex.ContainsId(x)).ToArray();
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