using System.Collections.Generic;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents schema to store and retrive wordlists informations.
    /// </summary>
    public class WordlistIndexLegacy
    {
        /// <summary>
        /// Gets the wordlist by the id.
        /// </summary>
        /// <param name="id">
        /// The id of the wordlist.
        /// </param>
        /// <returns></returns>
        public WordlistIndexEntryLegacy this[string id] => Wordlists[id];

        /// <summary>
        /// Gets or sets the wordlists container.
        /// </summary>
        public Dictionary<string, WordlistIndexEntryLegacy> Wordlists { get; set; } = new Dictionary<string, WordlistIndexEntryLegacy>();
    }
}
