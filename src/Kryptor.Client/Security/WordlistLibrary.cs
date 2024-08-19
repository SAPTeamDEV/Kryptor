using System.IO;

using SAPTeam.CommonTK;

namespace SAPTeam.Kryptor.Client.Security
{
    internal class WordlistLibrary
    {
        private WordlistIndexV2 remoteIndex;

        private Config<WordlistIndexV2> LocalIndexContainer { get; }

        private WordlistIndexV2 LocalIndex => LocalIndexContainer.Prefs;

        public string LocalIndexPath { get; }

        public string PreferredInstallPath { get; }

        public bool Indexing { get; }

        public WordlistIndexEntryV2[] Wordlists => LocalIndex.Wordlists.ToArray();

        public WordlistLibrary(string localIndexPath) : this(localIndexPath, null, false)
        {

        }

        public WordlistLibrary(string localIndexPath, WordlistIndexV2 remoteIndex, bool indexing)
        {
            LocalIndexPath = localIndexPath;
            var localIndexDir = Utilities.EnsureDirectoryExists(Path.GetDirectoryName(localIndexPath));

            LocalIndexContainer = new Config<WordlistIndexV2>(localIndexPath);
            this.remoteIndex = remoteIndex;

            Indexing = indexing;
            PreferredInstallPath = indexing ? Utilities.CreateTempFolder() : localIndexDir;
        }
    }
}
