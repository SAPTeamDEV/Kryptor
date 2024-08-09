using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

using SAPTeam.Kryptor.Client;
using System.Threading.Tasks;
using System.Threading;
using SAPTeam.Kryptor.Client.Security;
using System.Security.Policy;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistCompileSession : Session
    {
        Dictionary<string, FileStream> fileStreams = new Dictionary<string, FileStream>();

        string filePath;
        string destPath;

        public string Id;
        public WordlistIndexEntry IndexEntry;

        public WordlistCompileSession(string path, string destination, string id, WordlistIndexEntry entry)
        {
            filePath = path;
            destPath = destination;

            IndexEntry = entry;
            Id = id;
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                throw new InvalidOperationException("This file already compiled");
            }

            Description = $"Importing {Id}";
            long words = 0;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                double steps = (1.0 / streamReader.BaseStream.Length) * 100;

                string line;
                int readChars = 0;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    readChars += line.Length;

                    if (string.IsNullOrEmpty(line) || line.Length < 4) continue;

                    string c = Wordlist.GetWordIdentifier(line).ToString();
                    words++;

                    if (!fileStreams.ContainsKey(c))
                    {
                        fileStreams[c] = File.OpenWrite(Path.Combine(destPath, c + ".txt"));
                    }

                    var data = Encoding.UTF8.GetBytes(line + "\n");

                    fileStreams[c].Write(data, 0, data.Length);

                    Progress = readChars * steps;
                }
            }

            Progress = 100;

            foreach (var f in fileStreams.Values)
            {
                f.Flush();
                f.Dispose();
            }

            IndexEntry.InstallDirectory = destPath;
            IndexEntry.Words = words;
            return true;
        }
    }
}