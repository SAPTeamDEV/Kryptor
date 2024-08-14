using System;
using System.IO;
using System.Text;

using SAPTeam.CommonTK;
using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class IndexSessionHost : InstallSessionHost
    {
        private readonly string indexPath;

        private readonly string indexV2Path = "index.json";

        public override string LocalIndexPath => indexV2Path;

        public IndexSessionHost(GlobalOptions globalOptions, string indexPath) : base(globalOptions, list: false, all: true, recommended: false, ids: Array.Empty<string>())
        {
            this.indexPath = indexPath;
        }

        public override void Start(ClientContext context)
        {
            Config<WordlistIndex> IndexContainer = new Config<WordlistIndex>(indexPath);

            WordlistIndex Index = IndexContainer.Prefs;
            WordlistIndexV2 IndexV2 = new WordlistIndexV2();

            foreach (System.Collections.Generic.KeyValuePair<string, WordlistIndexEntry> entry in Index.Wordlists)
            {
                string id = entry.Key;
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
            base.Start(context);

            using (var f = File.OpenWrite("Index.md"))
            {
                var header = "# Wordlist Index\n\n| Identifier | Name | Words | Size | Download |\n| :--------: | :--: | :---: | :--: | :------: |\n";
                var buffer = Encoding.UTF8.GetBytes(header);
                f.Write(buffer, 0, buffer.Length);

                foreach (var entry in LocalIndex.Wordlists)
                {
                    var text = $"| {entry.Id} | {entry.Name} | {entry.Words} | {Utilities.ConvertBytes(entry.Size)} | [Download]({entry.Uri}) |\n";
                    buffer = Encoding.UTF8.GetBytes(text);
                    f.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}