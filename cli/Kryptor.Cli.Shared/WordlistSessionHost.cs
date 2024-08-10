using System;
using System.IO;
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

        public JsonSerializerSettings LocalIndexParserSettings { get; set; } = new JsonSerializerSettings();

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