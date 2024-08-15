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
        public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Wordlists/master/index.json");

        private readonly object _lockObj = new object();

        private readonly bool List;
        private readonly bool All;
        private readonly bool Recommended;
        private readonly string[] Ids;
        private readonly bool Indexing;

        public virtual string DownloadDir { get; } = Path.Combine(Program.Context.WordlistDirectory, "_cache");

        public virtual string InstallDir { get; } = Program.Context.WordlistDirectory;

        public WordlistIndexV2 Index { get; protected set; }

        public InstallSessionHost(GlobalOptions globalOptions, bool list, bool all, bool recommended, string[] ids) : base(globalOptions)
        {
            List = list;
            All = all;
            Recommended = recommended;
            Ids = ids;

            Indexing = this is IndexSessionHost;
            if (!Indexing)
            {
                Container.MaxRunningSessions = 2;
            }
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (!Directory.Exists(InstallDir))
            {
                Directory.CreateDirectory(InstallDir);
            }

            if (!Directory.Exists(DownloadDir))
            {
                Directory.CreateDirectory(DownloadDir);
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
                        Install(wordlist);
                    }
                }
                else if (Recommended)
                {
                    foreach (WordlistIndexEntryV2 wordlist in Index.Wordlists)
                    {
                        if (!wordlist.Enforced) continue;

                        Install(wordlist);
                    }
                }
                else
                {
                    foreach (string id in Ids)
                    {
                        Install(Index[id]);
                    }
                }

                ShowProgressMonitored(true, false).Wait();

                SortIndex();
            }
        }

        private void Install(WordlistIndexEntryV2 entry)
        {
            if (!GetInstallationPermission(entry))
            {
                return;
            }

            var downloadPath = Path.Combine(DownloadDir, entry.Id);
            var installPath = Path.Combine(InstallDir, entry.Id);

            DownloadSession downloader = new DownloadSession(entry, new DirectoryInfo(downloadPath));
            CompileSession compiler = new CompileSession(downloader.OutputFile.FullName, installPath, entry, indexing: Indexing, importing: false);
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