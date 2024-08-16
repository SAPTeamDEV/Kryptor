using System;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents schema to store and retrive wordlist informations.
    /// </summary>
    public class WordlistIndexEntryV2
    {
        /// <summary>
        /// Gets or sets the identifier of the wordlist.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly name of the wordlist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URI used to download the wordlist.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the SHA256 hash of the wordlist.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Gets or sets enforcement status of the wordlist. if it's true, it will block any operations if the word is found in the wordlist, but if set to false, it just shows a warning.
        /// </summary>
        public bool Enforced { get; set; }

        /// <summary>
        /// Gets or sets compressed status of the wordlist file. if it's true, the downloader will try to decompress it, otherwise it will be processed as is.
        /// </summary>
        public bool Compressed { get; set; }

        /// <summary>
        /// Gets or sets the install directory of thw wordlist.
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// Gets or sets the number of lines in this wordlist.
        /// </summary>
        public long Lines { get; set; }

        /// <summary>
        /// Gets or sets the number of actual words in this wordlist.
        /// </summary>
        public long Words { get; set; }

        /// <summary>
        /// Gets or sets the wordlist file size.
        /// </summary>
        public long Size { get; set; }
    }
}
