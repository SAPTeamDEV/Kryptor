using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets or sets the importance of the wordlist in the quick checks might be made by clients. 0 means the highest importance and 2 means lowest importance.
        /// </summary>
        public int Importance { get; set; }

        /// <summary>
        /// Gets or sets the install directory of thw wordlist.
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// Gets or sets the number of words in this wordlist.
        /// </summary>
        public long Words {  get; set; }
    }
}
