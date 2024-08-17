using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Provides a performance-friendly wordlist lookup class.
    /// </summary>
    public class WordlistFragmentCollection
    {
        /// <summary>
        /// Gets the path of wordlist directory.
        /// </summary>
        public string WordlistPath { get; }

        private Dictionary<int, HashSet<string>> Fragments { get; } = new Dictionary<int, HashSet<string>>();

        /// <summary>
        /// Initializes a new instance of <see cref="WordlistFragmentCollection"/> class.
        /// </summary>
        /// <param name="path">
        /// The path of compiled wordlist directory.
        /// </param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public WordlistFragmentCollection(string path)
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
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        /// <returns></returns>
        public async Task<bool> LookupAsync(string word, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int id = GetWordIdentifier(word);

            if (!Fragments.ContainsKey(id))
            {
                string fragmentPath = Path.Combine(WordlistPath, id.ToString());
                if (!File.Exists(fragmentPath))
                {
                    return false;
                }

                HashSet<string> hashes = await InitializeFragment(fragmentPath, cancellationToken);

                Fragments.Add(id, hashes);
            }

            return Fragments[id].Contains(word);
        }

        private static async Task<HashSet<string>> InitializeFragment(string lPath, CancellationToken cancellationToken)
        {
            HashSet<string> hashes = new HashSet<string>();

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

            return hashes;
        }

        /// <summary>
        /// Gets the numeric fragment identifier of the given <paramref name="word"/>.
        /// </summary>
        /// <param name="word">
        /// The word to calculate.
        /// </param>
        /// <returns></returns>
        public static int GetWordIdentifier(string word)
        {
            int fragmentId = 0;

            foreach (int c in word)
            {
                fragmentId = (fragmentId + Math.Abs(c)) % 0x4000000;
            }

            return fragmentId % 1000;
        }
    }
}