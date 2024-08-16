using System;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents schema to store and retrive wordlist informations.
    /// </summary>
    public class WordlistIndexEntry
    {
        /// <summary>
        /// Gets or sets the user-friendly name of the wordlist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URI used to download the wordlist.
        /// </summary>
        public Uri DownloadUri { get; set; }

        /// <summary>
        /// Gets or sets the priority of the wordlist in the quick checks might be made by clients. 0 means highest priority and 2 means lowest priority.
        /// </summary>
        public int QuickCheckPriority { get; set; }

        /// <summary>
        /// Gets or sets whether this wordlist is compressed.
        /// </summary>
        public bool Compressed { get; set; }
    }
}
