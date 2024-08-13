using System.Linq;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class RemoveSessionHost : SessionHost
    {
        private readonly bool List;
        private readonly bool All;
        private string[] Wordlists;

        public RemoveSessionHost(GlobalOptions globalOptions, bool list, bool all, string[] wordlists) : base(globalOptions)
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
                    LogError("There is no installed wordlist to remove");
                    return;
                }
            }
            else
            {
                Wordlists = Wordlists.Where(LocalIndex.ContainsId).ToArray();
                if (Wordlists.Length == 0)
                {
                    LogError("No valid id are supplied");
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