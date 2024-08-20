using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Provides a performance-friendly wordlist lookup class.
    /// </summary>
    public class WordlistFragmentCollection
    {
        private Dictionary<int, HashSet<string>> Fragments { get; } = new Dictionary<int, HashSet<string>>();
        private WordlistIndexEntry IndexEntry { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="WordlistFragmentCollection"/> class.
        /// </summary>
        /// <param name="entry">
        /// A valid wordlist entry.
        /// </param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public WordlistFragmentCollection(WordlistIndexEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (string.IsNullOrEmpty(entry.Id)
                || string.IsNullOrEmpty(entry.InstallDirectory)
                || !Directory.Exists(entry.InstallDirectory)
                || entry.Hash == null
                || entry.Hash.Length != 32)
            {
                throw new ArgumentException("Invalid wordlist entry");
            }

            IndexEntry = entry;
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
                string fragmentPath = Path.Combine(IndexEntry.InstallDirectory, id.ToString());
                if (!File.Exists(fragmentPath))
                {
                    return false;
                }

                HashSet<string> hashes = await InitializeFragment(id, fragmentPath, cancellationToken);

                Fragments.Add(id, hashes);
            }

            return Fragments[id].Contains(word);
        }

        private async Task<HashSet<string>> InitializeFragment(int id, string lPath, CancellationToken cancellationToken)
        {
            HashSet<string> fragment = new HashSet<string>();
            var metadata = IndexEntry.GetMetadata().Where(x => x.FragmentId == id).First();
            bool isCompatible = false;

            using (StreamReader streamReader = new StreamReader(lPath, Encoding.UTF8))
            {
                VerifyFragment(streamReader.BaseStream, metadata);

                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    line = line.Trim();

                    if (line == metadata.LookupString) isCompatible = true;

                    fragment.Add(line);
                }
            }

            if (!isCompatible)
            {
                throw new NotSupportedException("The wordlist is not compatible with this version");
            }

            return fragment;
        }

        private void VerifyFragment(Stream stream, WordlistVerificationMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (stream == null) throw new ArgumentNullException("stream");

            var fragmentHash = stream.Sha256();
            var fragmentChecksum = Utilities.XOR(IndexEntry.Hash, fragmentHash);

            if (!metadata.Checksum.SequenceEqual(fragmentChecksum))
            {
                throw new InvalidDataException("Wordlist fragment is corrupted");
            }
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