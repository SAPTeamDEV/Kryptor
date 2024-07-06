using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SAPTeam.Kryptor.Security
{
    static class Wordlist
    {
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
    }
}