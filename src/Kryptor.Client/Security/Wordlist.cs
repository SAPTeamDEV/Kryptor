using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Provides a performance-friendly wordlist checker.
    /// </summary>
    public class Wordlist
    {
        /// <summary>
        /// Gets the path of wordlist directory.
        /// </summary>
        public string WordlistPath { get; }

        private Dictionary<int, HashSet<string>> Subsets { get; } = new Dictionary<int, HashSet<string>>();

        /// <summary>
        /// Initializes a new instance of <see cref="Wordlist"/> class.
        /// </summary>
        /// <param name="path">
        /// The path of well formatted wordlist directory.
        /// </param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public Wordlist(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(path);
            }

            WordlistPath = path;
        }

        /// <summary>
        /// Checks whether the <paramref name="word"/> is found in the wordlist.
        /// </summary>
        /// <param name="word">
        /// Word to search.
        /// </param>
        /// <returns></returns>
        public async Task<bool> ContainsAsync(string word, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int id = GetWordIdentifier(word);

            if (!Subsets.ContainsKey(id))
            {
                var lPath = Path.Combine(WordlistPath, id.ToString() + ".txt");
                if (!File.Exists(lPath))
                {
                    return false;
                }

                var hashes = new HashSet<string>();

                using (StreamReader streamReader = new StreamReader(lPath, Encoding.UTF8))
                {
                    string line;
                    while ((line = await streamReader.ReadLineAsync()) != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        hashes.Add(line.Trim());
                    }
                }

                Subsets.Add(id, hashes);
            }

            return Subsets[id].Contains(word);
        }

        /// <summary>
        /// Gets the numeric category identifier of given <paramref name="word"/>.
        /// </summary>
        /// <param name="word">
        /// The word to calculate.
        /// </param>
        /// <returns></returns>
        public static int GetWordIdentifier(string word)
        {
            return Math.Abs(word[0] + word[1] + word[2]) % 64;
        }
    }
}