using System;
using System.IO;
using System.Linq;
using System.Net.Http;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistInstallSessionHost : WordlistSessionHost
    {
        public override int MaxRunningSessions => 2;

        private readonly bool List;
        private readonly bool All;
        private readonly bool Recommended;
        private readonly string[] Ids;
        private readonly bool Converting;

        /* Unmerged change from project 'Kryptor.Cli.Legacy (net472)'
        Before:
                static public Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        After:
                public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        */

        /* Unmerged change from project 'Kryptor.Cli.Legacy (net481)'
        Before:
                static public Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        After:
                public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        */

        /* Unmerged change from project 'Kryptor.Cli (net8.0)'
        Before:
                static public Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        After:
                public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        */

        /* Unmerged change from project 'Kryptor.Cli (net6.0)'
        Before:
                static public Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        After:
                public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");
        */
        public static Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");

        public WordlistIndexV2 Index { get; protected set; }

        public WordlistInstallSessionHost(bool verbose, bool list, bool all, bool recommended, string[] ids) : base(verbose)
        {
            List = list;
            All = all;
            Recommended = recommended;
            Ids = ids;

            Converting = this is WordlistConverterSessionHost;
        }

        public override void Start()
        {
            base.Start();

            if (Converting)
            {
                Log("Converting v1 index to v2");
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

                foreach (ISession session in Container.Sessions)
                {
                    if (session is WordlistCompileSession compiler && compiler.Status == SessionStatus.Ended && compiler.EndReason == SessionEndReason.Completed)
                    {
                        DebugLog($"Adding {compiler.IndexEntry.Id} to local index");

                        try
                        {
                            LocalIndex.Add(compiler.IndexEntry);
                        }
                        catch
                        {
                            Log($"Cannot add {compiler.IndexEntry.Id}");
                        }

                        try
                        {
                            if (File.Exists(compiler.FilePath))
                            {
                                File.Delete(compiler.FilePath);
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                UpdateLocalIndex();
            }
        }

        private void Install(string id)
        {
            if (!GetInstallationPermission(Index[id]))
            {
                return;
            }

            WordlistDownloadSession downloader = new WordlistDownloadSession(Index[id].Uri, id);

            string localRepo = Converting ? Path.Combine(Program.Context.WordlistDirectory, "_temp") : Program.Context.WordlistDirectory;

            WordlistCompileSession compiler = new WordlistCompileSession(downloader.FilePath, Path.Combine(localRepo, id), Index[id], converting: Converting, importing: false);
            compiler.Dependencies.Add(downloader);

            NewSession(downloader);
            NewSession(compiler);
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