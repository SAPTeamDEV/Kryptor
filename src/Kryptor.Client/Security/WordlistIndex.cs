using System;
using System.Collections.Generic;
using System.Linq;

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
        public WordlistIndexEntry this[string id]
        {
            get
            {
                try
                {
                    return Wordlists.First(x => x.Id == id);
                }
                catch (InvalidOperationException)
                {
                    throw new KeyNotFoundException(id);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wordlists container.
        /// </summary>
        public List<WordlistIndexEntry> Wordlists { get; set; } = new List<WordlistIndexEntry>();

        /// <summary>
        /// Safely Adds new entry to the index.
        /// </summary>
        /// <param name="entry">
        /// The wordlist entry.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public void Add(WordlistIndexEntry entry)
        {
            if (ContainsId(entry.Id))
            {
                throw new ArgumentException($"The id {entry.Id} is already exists");
            }

            if (entry.Hash != null)
            {
                foreach (WordlistIndexEntry e in Wordlists.Where(x => x.Hash != null))
                {
                    if (e.Hash.SequenceEqual(entry.Hash))
                    {
                        throw new ArgumentException("A wordlist with this hash already exists");
                    }
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
        public bool ContainsId(string id) => Wordlists.Where(x => x.Id == id).Count() > 0;
    }
}
