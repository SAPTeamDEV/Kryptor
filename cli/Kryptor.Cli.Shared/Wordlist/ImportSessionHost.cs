using System.IO;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class ImportSessionHost : SessionHost
    {
        private readonly WordlistIndexEntry IndexEntry;
        private readonly string FilePath;
        private readonly bool Optimize;

        public ImportSessionHost(GlobalOptions globalOptions, string id, bool enforce, bool optimize, string file) : base(globalOptions)
        {
            IndexEntry = new WordlistIndexEntry()
            {
                Id = id,
                Enforced = enforce
            };

            Optimize = optimize;

            FilePath = file;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            using (FileStream f = File.OpenRead(FilePath))
            {
                IndexEntry.Size = f.Length;

                IndexEntry.Hash = f.Sha256();
            }

            if (GetInstallationPermission(IndexEntry))
            {
                CompileSession compiler = new CompileSession(FilePath, Path.Combine(Program.Context.WordlistDirectory, IndexEntry.Id), IndexEntry, optimize: Optimize, indexing: false, importing: true);
                NewSession(compiler);

                ShowProgressMonitored(true).Wait();

                SortIndex();
            }
        }
    }
}