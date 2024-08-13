using System.IO;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class ImportSessionHost : SessionHost
    {
        private readonly WordlistIndexEntryV2 IndexEntry;
        private readonly string FilePath;

        public ImportSessionHost(GlobalOptions globalOptions, string id, bool enforce, string file) : base(globalOptions)
        {
            IndexEntry = new WordlistIndexEntryV2()
            {
                Id = id,
                Enforced = enforce
            };

            FilePath = file;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            using (FileStream f = File.OpenRead(FilePath))
            {
                IndexEntry.Size = f.Length;

                byte[] buffer = new byte[f.Length];
                f.Read(buffer, 0, buffer.Length);

                IndexEntry.Hash = buffer.Sha256();
            }

            if (GetInstallationPermission(IndexEntry))
            {
                CompileSession compiler = new CompileSession(FilePath, Path.Combine(Program.Context.WordlistDirectory, IndexEntry.Id), IndexEntry, converting: false, importing: true);
                NewSession(compiler);

                ShowProgressMonitored(true).Wait();

                SortIndex();
            }
        }
    }
}