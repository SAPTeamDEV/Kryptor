using System;
using System.IO;
using System.Linq;
using System.Net.Http;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class InstallSessionHost : SessionHost
    {
        public override int MaxRunningSessions => 2;

        private object _lockObj = new object();

        private readonly bool List;
        private readonly bool All;
        private readonly bool Recommended;
        private readonly string[] Ids;
        private readonly bool Converting;

        public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");

        public WordlistIndexV2 Index { get; protected set; }

        public InstallSessionHost(GlobalOptions globalOptions, bool list, bool all, bool recommended, string[] ids) : base(globalOptions)
        {
            List = list;
            All = all;
            Recommended = recommended;
            Ids = ids;

            Converting = this is ConverterSessionHost;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (Converting)
            {
                DebugLog("Converting v1 index to v2");
            }

            if (Index == null)
            {
                DebugLog("Getting wordlist index...");

                HttpClient client = new HttpClient();
                string rawIndex = client.GetStringAsync(WordlistIndexUri).Result;

                Index = JsonConvert.DeserializeObject<WordlistIndexV2>(rawIndex);
            }

            if (List || (!All && !Recommended && Ids.Length == 0))
            {
                PrintList();
            }
            else
            {
                if (All)
                {
                    foreach (WordlistIndexEntryV2 wordlist in Index.Wordlists)
                    {
                        Install(wordlist.Id);
                    }
                }
                else if (Recommended)
                {
                    foreach (WordlistIndexEntryV2 wordlist in Index.Wordlists)
                    {
                        if (!wordlist.Enforced) continue;

                        Install(wordlist.Id);
                    }
                }
                else
                {
                    foreach (string id in Ids)
                    {
                        Install(id);
                    }
                }

                ShowProgressMonitored(true, false).Wait();

                SortIndex();
            }
        }

        private void Install(string id)
        {
            if (!GetInstallationPermission(Index[id]))
            {
                return;
            }

            DownloadSession downloader = new DownloadSession(Index[id].Uri, id);

            string localRepo = Converting ? Path.Combine(Program.Context.WordlistDirectory, "_temp") : Program.Context.WordlistDirectory;

            CompileSession compiler = new CompileSession(downloader.FilePath, Path.Combine(localRepo, id), Index[id], converting: Converting, importing: false);
            downloader.ContinueWith(compiler);

            NewSession(downloader);
            NewSession(compiler);
        }

        public void FinalizeInstallation(WordlistIndexEntryV2 entry)
        {
            lock (_lockObj)
            {
                LocalIndex.Add(entry);
                UpdateLocalIndex();
            }
        }

        private void PrintList()
        {
            foreach (WordlistIndexEntryV2 wordlist in Index.Wordlists)
            {
                string status = !LocalIndex.ContainsId(wordlist.Id) ? "" : LocalIndex[wordlist.Id].Hash.SequenceEqual(wordlist.Hash) ? "(Installed)" : "(Update Avaiable)";

                Log($"\n{wordlist.Id}: {status}");
                Log($"Description: {wordlist.Name}");
                Log($"Download Size: {Utilities.ConvertBytes(wordlist.Size)}");
            }
        }
    }
}