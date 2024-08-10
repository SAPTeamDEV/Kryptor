using System;
using System.IO;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistConverterSessionHost : WordlistInstallSessionHost
    {
        string indexPath;
        string indexV2Path;

        public override string LocalIndexPath => indexV2Path;

        public WordlistConverterSessionHost(bool verbose, string indexPath, string indexV2Path) : base(verbose, list: false, all: true, recommended: false, ids: Array.Empty<string>())
        {
            this.indexPath = indexPath;
            this.indexV2Path = indexV2Path;
        }

        public override void Start()
        {
            var IndexContainer = new Config<WordlistIndex>(indexPath);

            var Index = IndexContainer.Prefs;
            var IndexV2 = new WordlistIndexV2();

            foreach (var entry in Index.Wordlists)
            {
                var id = entry.Key;
                WordlistIndexEntryV2 entryV2;

                if (IndexV2.ContainsId(id))
                {
                    entryV2 = IndexV2[id];
                }
                else
                {
                    entryV2 = new WordlistIndexEntryV2()
                    {
                        Id = id,
                    };

                    IndexV2.Add(entryV2);
                }

                entryV2.Name = entry.Value.Name;
                entryV2.Uri = entry.Value.DownloadUri;
                entryV2.Enforced = entry.Value.QuickCheckPriority == 0;
            }

            if (File.Exists(indexV2Path))
            {
                File.Delete(indexV2Path);
            }

            this.Index = IndexV2;
            base.Start();
        }
    }
}