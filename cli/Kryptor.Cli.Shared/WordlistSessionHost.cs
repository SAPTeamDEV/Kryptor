using System;
using System.IO;

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
            else
            {
                Log("This feature is not implemented yet");
            }
        }
    }
}