using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents schema to store and retrive wordlists informations.
    /// </summary>
    public class WordlistIndexV2
    {
        /// <summary>
        /// Gets the wordlist by the id.
        /// </summary>
        /// <param name="id">
        /// The id of the wordlist.
        /// </param>
        /// <returns></returns>
        public WordlistIndexEntryV2 this[string id]
        {
            get
            {
                return Wordlists.Where(x => x.Id == id).First();
            }
        }

        /// <summary>
        /// Gets or sets the wordlists container.
        /// </summary>
        public List<WordlistIndexEntryV2> Wordlists { get; set; } = new List<WordlistIndexEntryV2>();

        /// <summary>
        /// Safely Adds new entry to the index.
        /// </summary>
        /// <param name="entry">
        /// The wordlist entry.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public void Add(WordlistIndexEntryV2 entry)
        {
            if (ContainsId(entry.Id))
            {
                throw new ArgumentException($"The id {entry.Id} is already exists");
            }

            if (entry.Hash != null)
            {
                foreach (var e in Wordlists.Where(x => x.Hash != null))
                {
                    e.Hash.SequenceEqual(entry.Hash);
                    throw new ArgumentException("A wordlist with this hash already exists");
                }
            }

            Wordlists.Add(entry);
        }

        /// <summary>
        /// Determines whether the index contains the specified id.
        /// </summary>
        /// <param name="id">
        /// The id to search.
        /// </param>
        /// <returns></returns>
        public bool ContainsId(string id)
        {
            return Wordlists.Where(x => x.Id == id).Count() > 0;
        }
    }
}
