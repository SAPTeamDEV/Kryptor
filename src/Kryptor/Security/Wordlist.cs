using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

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
                    if (string.IsNullOrEmpty(line)) continue;

                    hashes.Add(line.Trim());
                }

                Subsets.Add(id, hashes);
            }

            return Subsets[id].Contains(word);
        }

        public static int GetWordIdentifier(string word) => Math.Abs(word[0] + word[1] + word[2]) % 64;

        /*
        static Dictionary<string, FileStream> fileStreams = new Dictionary<string, FileStream>();

        public static void Main(string[] args)
        {
            string fileName = args[0];

            foreach (var line in File.ReadAllLines(fileName))
            {
                if (string.IsNullOrEmpty(line) || line.Length < 4) continue;
                string c = (Math.Abs(line[0] + line[1] + line[2]) % 64).ToString();
                if (!fileStreams.ContainsKey(c))
                {
                    fileStreams[c] = File.OpenWrite(c + ".txt");
                }
                var data = Encoding.UTF8.GetBytes(line + "\n");

                fileStreams[c].Write(data, 0, data.Length);
            }

            foreach (var f in fileStreams.Values)
            {
                f.Flush();
                f.Dispose();
            }
        }
        */
    }
}