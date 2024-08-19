using System.IO;

using SAPTeam.CommonTK;

namespace SAPTeam.Kryptor.Client.Security
{
    internal class WordlistLibrary
    {
        private WordlistIndex remoteIndex;

        private Config<WordlistIndex> LocalIndexContainer { get; }

        private WordlistIndex LocalIndex => LocalIndexContainer.Prefs;

        public string LocalIndexPath { get; }

        public string PreferredInstallPath { get; }

        public bool Indexing { get; }

        public WordlistIndexEntry[] Wordlists => LocalIndex.Wordlists.ToArray();

        public WordlistLibrary(string localIndexPath) : this(localIndexPath, null, false)
        {

        }

        public WordlistLibrary(string localIndexPath, WordlistIndex remoteIndex, bool indexing)
        {
            LocalIndexPath = localIndexPath;
            var localIndexDir = Utilities.EnsureDirectoryExists(Path.GetDirectoryName(localIndexPath));

            LocalIndexContainer = new Config<WordlistIndex>(localIndexPath);
            this.remoteIndex = remoteIndex;

            Indexing = indexing;
            PreferredInstallPath = indexing ? Utilities.CreateTempFolder() : localIndexDir;
        }
    }
}
