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
        string fileName;

        public WordlistCompileSession(string path)
        {
            fileName = path;
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            string dirName = Path.GetFileName(fileName) + ".compiled";

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            else
            {
                throw new InvalidOperationException("This file already compiled");
            }

            Description = Path.GetFullPath(dirName);

            using (StreamReader streamReader = new StreamReader(fileName, Encoding.UTF8))
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

                    if (!fileStreams.ContainsKey(c))
                    {
                        fileStreams[c] = File.OpenWrite(Path.Combine(dirName, c + ".txt"));
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

            return true;
        }
    }
}