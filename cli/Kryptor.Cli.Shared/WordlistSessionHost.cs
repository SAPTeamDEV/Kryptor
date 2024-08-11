using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistSessionHost : CliSessionHost
    {
        Config<WordlistIndexV2> LocalIndexContainer { get; set; }
        public WordlistIndexV2 LocalIndex => LocalIndexContainer.Prefs;

        public virtual string LocalIndexPath => Path.Combine(Program.Context.WordlistDirectory, "index.json");

        public JsonSerializerSettings LocalIndexParserSettings { get; set; } = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public WordlistSessionHost(bool verbose) : base(verbose)
        {
            LocalIndexParserSettings.Converters.Add(new SchemaJsonConverter("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/schema-v2.json"));
        }

        public override void Start()
        {
            base.Start();

            DebugLog("Loading local index...");
            LocalIndexContainer = new Config<WordlistIndexV2>(LocalIndexPath, LocalIndexParserSettings);
        }

        protected void UpdateLocalIndex()
        {
            LocalIndexContainer.Write();
        }

        protected void ListInstalledWordlists()
        {
            foreach (var wl in LocalIndex.Wordlists)
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
                    Log($"{entry.Id} is already installed");
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
            var entry = LocalIndex[id];

            if (entry.InstallDirectory != null)
            {
                Directory.Delete(entry.InstallDirectory, true);
            }

            LocalIndex.Wordlists.Remove(entry);

            UpdateLocalIndex();
        }
    }
}