using System.Collections.Generic;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents schema to store and retrive wordlists informations.
    /// </summary>
    public class WordlistIndex
    {
        /// <summary>
        /// Gets the wordlist by the id.
        /// </summary>
        /// <param name="id">
        /// The id of the wordlist.
        /// </param>
        /// <returns></returns>
        public WordlistIndexEntry this[string id] => Wordlists[id];

        /// <summary>
        /// Gets or sets the wordlists container.
        /// </summary>
        public Dictionary<string, WordlistIndexEntry> Wordlists { get; set; } = new Dictionary<string, WordlistIndexEntry>();
    }
}
