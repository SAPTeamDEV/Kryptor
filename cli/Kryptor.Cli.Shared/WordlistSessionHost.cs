using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;

using Newtonsoft.Json;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistSessionHost : CliSessionHost
    {
        Config<WordlistIndex> LocalIndexContainer { get; set; }
        public WordlistIndex LocalIndex => LocalIndexContainer.Prefs;

        public string LocalIndexPath { get; } = Path.Combine(Program.Context.WordlistDirectory, "index.json");

        public WordlistSessionHost(bool verbose) : base(verbose)
        {

        }

        public override void Start()
        {
            base.Start();

            DebugLog("Loading local index...");
            LocalIndexContainer = new Config<WordlistIndex>(LocalIndexPath);
        }

        protected void UpdateLocalIndex()
        {
            LocalIndexContainer.Write();
        }
    }
}