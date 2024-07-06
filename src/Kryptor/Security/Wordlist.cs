using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SAPTeam.Kryptor.Security
{
    public class Wordlist
    {
        public string WordlistPath { get; }

        public Dictionary<int, HashSet<string>> Subsets { get; } = new Dictionary<int, HashSet<string>>();

        public Wordlist(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(path);
            }

            WordlistPath = path;
        }

        public bool Contains(string word)
        {
            int id = GetWordIdentifier(word);

            if (!Subsets.ContainsKey(id))
            {
                var lPath = Path.Combine(WordlistPath, id.ToString() + ".txt");
                if (!File.Exists(lPath))
                {
                    return false;
                }

                var hashes = new HashSet<string>();

                foreach (var line in File.ReadAllLines(lPath))
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    hashes.Add(line.Trim());
                }

                Subsets.Add(id, hashes);
            }

            return Subsets[id].Contains(word);
        }

        public static int GetWordIdentifier(string word)
        {
            return Math.Abs(word[0] + word[1] + word[2]) % 64;
        }
    }
}