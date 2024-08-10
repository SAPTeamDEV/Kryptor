using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistInstallSessionHost : WordlistSessionHost
    {
        public override int MaxRunningSessions => 2;

        bool List;
        bool All;
        bool Recommended;
        string[] Ids;

        bool Converting;

        static public Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-IndexV2.json");

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

                var client = new HttpClient();
                var rawIndex = client.GetStringAsync(WordlistIndexUri).Result;

                Index = JsonConvert.DeserializeObject<WordlistIndexV2>(rawIndex);
            }

            if (List)
            {
                PrintList();
            }
            else
            {
                if (All)
                {
                    foreach (var wordlist in Index.Wordlists)
                    {
                        Install(wordlist.Id);
                    }
                }
                else if (Recommended)
                {
                    foreach (var wordlist in Index.Wordlists)
                    {
                        if (wordlist.Importance > 0) continue;

                        Install(wordlist.Id);
                    }
                }
                else
                {
                    foreach (var id in Ids)
                    {
                        Install(id);
                    }
                }

                ShowProgressMonitored(true).Wait();

                foreach (var session in Container.Sessions)
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
            if (LocalIndex.ContainsId(id))
            {
                if (LocalIndex[id].Hash.SequenceEqual(Index[id].Hash))
                {
                    Log($"{id} already installed");
                    return;
                }
                else
                {
                    RemoveWordlist(id);
                }
            }

            var downloader = new WordlistDownloadSession(Index[id].Uri, id);

            var localRepo = Converting ? Path.Combine(Program.Context.WordlistDirectory, "_temp") : Program.Context.WordlistDirectory;

            var compiler = new WordlistCompileSession(downloader.FilePath, Path.Combine(localRepo, id), Index[id], Converting);
            compiler.SessionDependencies.Add(downloader);

            NewSession(downloader);
            NewSession(compiler);
        }

        void PrintList()
        {
            foreach (var wordlist in Index.Wordlists)
            {
                var status = !LocalIndex.ContainsId(wordlist.Id) ? "" : LocalIndex[wordlist.Id].Hash.SequenceEqual(wordlist.Hash) ? "(Installed)" : "(Update Avaiable)";

                Log($"\n{wordlist.Id}: {status}");
                Log($"Description: {wordlist.Name}");

                var request = WebRequest.CreateHttp(wordlist.Uri);
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
                request.Method = "HEAD";

                string length;

                try
                {
                    using (var response = request.GetResponseAsync().Result)
                    {
                        length = $"{Utilities.ConvertBytes(response.ContentLength)}";
                    }
                }
                catch
                {
                    length = "N/A";
                }

                Log($"Download Size: {length}");
            }
        }
    }
}