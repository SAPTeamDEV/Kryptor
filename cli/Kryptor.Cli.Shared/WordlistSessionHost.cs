using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistSessionHost : CliSessionHost
    {
        int Mode;

        int QueryMode = 0;
        int CompileMode = 1;
        int InstallMode = 2;
        int RemoveMode = 3;

        string Query;
        string Compile;

        static public Uri WordlistIndex { get; } = new Uri("https://raw.githubusercontent.com/SAPTeamDEV/Kryptor/master/Wordlist-Index.json");

        public WordlistSessionHost(bool verbose, string query, string compile, bool install, bool remove) : base(verbose)
        {
            if (!string.IsNullOrEmpty(query))
            {
                Mode = QueryMode;
                Query = query;
            }
            else if (!string.IsNullOrEmpty(compile))
            {
                Mode = CompileMode;
                Compile = compile;
            }
            else if (install)
            {
                Mode = InstallMode;
            }
            else if (remove)
            {
                Mode = RemoveMode;
            }
            else
            {
                throw new ArgumentException("You must specify one of -q <word>, -i or -r options");
            }
        }

        public override void Start()
        {
            base.Start();

            if (Mode == QueryMode)
            {
                foreach (var dir in Directory.EnumerateDirectories(Program.Context.WordlistDirectory))
                {
                    var session = new WordlistQuerySession(dir, Query);
                    NewSession(session);
                }

                ShowProgressMonitored(true).Wait();
            }
            else if (Mode == CompileMode)
            {
                var session = new WordlistCompileSession(Compile);
                NewSession(session);
                ShowProgressMonitored(true).Wait();
            }
            else if (Mode == InstallMode)
            {
                Log("Getting wordlist index...");

                var client = new HttpClient();
                var rawIndex = client.GetStringAsync(WordlistIndex).Result;
                
                var index = JsonConvert.DeserializeObject<WordlistIndex>(rawIndex);

                foreach (var wordlist in index.Wordlists)
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
                            length = $"{response.ContentLength / 1024}KB";
                        }
                    }
                    catch
                    {
                        length = "N/A";
                    }


                    Log($"Download Size: {length}");
                }

                Log("\nThis feature is not completed yet");
            }
            else
            {
                Log("This feature is not implemented yet");
            }
        }
    }
}