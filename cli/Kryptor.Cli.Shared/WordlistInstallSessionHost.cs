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

        static public Uri WordlistIndexUri { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-Index.json");

        public WordlistIndex Index { get; private set; }

        public WordlistInstallSessionHost(bool verbose, bool list, bool all, bool recommended, string[] ids) : base(verbose)
        {
            List = list;
            All = all;
            Recommended = recommended;
            Ids = ids;
        }

        public override void Start()
        {
            base.Start();

            DebugLog("Getting wordlist index...");

            var client = new HttpClient();
            var rawIndex = client.GetStringAsync(WordlistIndexUri).Result;

            Index = JsonConvert.DeserializeObject<WordlistIndex>(rawIndex);

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
                        Install(wordlist.Key);
                    }
                }
                else if (Recommended)
                {
                    foreach (var wordlist in Index.Wordlists)
                    {
                        if (wordlist.Value.QuickCheckPriority > 0) continue;

                        Install(wordlist.Key);
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
                        DebugLog($"Adding {compiler.Id} to local index");
                        LocalIndex.Wordlists.Add(compiler.Id, compiler.IndexEntry);
                    }
                }

                UpdateLocalIndex();
            }
        }

        private void Install(string id)
        {
            if (LocalIndex.Wordlists.ContainsKey(id))
            {
                Log($"{id} already installed");
                return;
            }

            var downloader = new WordlistDownloadSession(Index.Wordlists[id].DownloadUri, id);
            var compiler = new WordlistCompileSession(downloader.FilePath, Path.Combine(Program.Context.WordlistDirectory, id), id, Index[id]);
            compiler.SessionDependencies.Add(downloader);

            NewSession(downloader);
            NewSession(compiler);
        }

        void PrintList()
        {
            foreach (var wordlist in Index.Wordlists)
            {
                Log($"\n{wordlist.Key}:");
                Log($"Description: {wordlist.Value.Name}");

                var request = WebRequest.CreateHttp(wordlist.Value.DownloadUri);
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