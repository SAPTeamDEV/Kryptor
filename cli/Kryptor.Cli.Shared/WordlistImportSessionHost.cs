using System.IO;

using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistImportSessionHost : WordlistSessionHost
    {
        private readonly WordlistIndexEntryV2 IndexEntry;
        private readonly string FilePath;

        public WordlistImportSessionHost(bool verbose, string id, bool enforce, string file) : base(verbose)
        {
            IndexEntry = new WordlistIndexEntryV2()
            {
                Id = id,
                Enforced = enforce
            };

            FilePath = file;
        }

        public override void Start()
        {
            base.Start();

            using (var f = File.OpenRead(FilePath))
            {
                IndexEntry.Size = f.Length;

                byte[] buffer = new byte[f.Length];
                f.Read(buffer, 0, buffer.Length);

                IndexEntry.Hash = buffer.Sha256();
            }

            if (GetInstallationPermission(IndexEntry))
            {
                var compiler = new WordlistCompileSession(FilePath, Path.Combine(Program.Context.WordlistDirectory, IndexEntry.Id), IndexEntry, converting: false, importing: true);
                NewSession(compiler);
                ShowProgressMonitored(true).Wait();
            }
        }
    }
}