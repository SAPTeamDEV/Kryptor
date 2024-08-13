using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class SessionHost : CliSessionHost
    {
        private Config<WordlistIndexV2> LocalIndexContainer { get; set; }
        public WordlistIndexV2 LocalIndex => LocalIndexContainer.Prefs;

        public virtual string LocalIndexPath => Path.Combine(Program.Context.WordlistDirectory, "index.json");

        public JsonSerializerSettings LocalIndexParserSettings { get; set; } = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public SessionHost(GlobalOptions globalOptions) : base(globalOptions) => LocalIndexParserSettings.Converters.Add(new SchemaJsonConverter("https://raw.githubusercontent.com/SAPTeamDEV/Wordlists/master/schema-v2.json"));

        public override void Start(ClientContext context)
        {
            base.Start(context);

            DebugLog("Loading local index...");
            LocalIndexContainer = new Config<WordlistIndexV2>(LocalIndexPath, LocalIndexParserSettings);
        }

        protected void UpdateLocalIndex() => LocalIndexContainer.Write();

        protected void ListInstalledWordlists()
        {
            if (LocalIndex.Wordlists.Count == 0)
            {
                LogError("No wordlists are installed");
                return;
            }

            foreach (WordlistIndexEntryV2 wl in LocalIndex.Wordlists)
            {
                Log($"\n{wl.Id}:");
                Log($"Description: {wl.Name}");
                Log($"Enforced: {wl.Enforced}");
                Log($"Words: {wl.Words}");
            }
        }

        protected bool GetInstallationPermission(WordlistIndexEntryV2 entry)
        {
            if (LocalIndex.ContainsId(entry.Id))
            {
                if (LocalIndex[entry.Id].Hash.SequenceEqual(entry.Hash))
                {
                    LogError($"{entry.Id} is already installed");
                    return false;
                }
                else
                {
                    RemoveWordlist(entry.Id);
                }
            }

            return true;
        }

        protected void RemoveWordlist(string id)
        {
            WordlistIndexEntryV2 entry = LocalIndex[id];

            if (entry.InstallDirectory != null && Directory.Exists(entry.InstallDirectory))
            {
                Directory.Delete(entry.InstallDirectory, true);
            }

            LocalIndex.Wordlists.Remove(entry);

            UpdateLocalIndex();
        }

        protected void SortIndex()
        {
            LocalIndex.Wordlists = LocalIndex.Wordlists.OrderBy(x => x.Id).ToList();
            UpdateLocalIndex();
        }
    }
}