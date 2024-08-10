using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

using SAPTeam.Kryptor.Client;
using System.Threading.Tasks;
using System.Threading;
using SAPTeam.Kryptor.Client.Security;
using System.Security.Policy;
using SAPTeam.Kryptor.Extensions;
using System.Linq;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistCompileSession : Session
    {
        Dictionary<string, FileStream> fileStreams = new Dictionary<string, FileStream>();

        string filePath;
        string destPath;

        public WordlistIndexEntryV2 IndexEntry;

        bool Converting;

        public WordlistCompileSession(string path, string destination, WordlistIndexEntryV2 entry, bool converting)
        {
            filePath = path;
            destPath = destination;

            IndexEntry = entry;

            Converting = converting;
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
            else if (!Converting)
            {
                throw new InvalidOperationException("This file already compiled");
            }

            Description = $"Importing {IndexEntry.Id}";
            long words = 0;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                try
                {
                    var buffer = new byte[streamReader.BaseStream.Length];
                    await streamReader.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                    var hash = buffer.Sha256();

                    if (Converting)
                    {
                        IndexEntry.Hash = hash;
                    }
                    else if (IndexEntry.Hash != null)
                    {
                        if (!IndexEntry.Hash.SequenceEqual(hash))
                        {
                            throw new InvalidDataException("Downloaded file is corrupted");
                        }
                    }
                }
                catch (InvalidDataException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // Ignore hash errors
                }
                
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

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

            if (!Converting)
            {
                IndexEntry.InstallDirectory = destPath;
            }

            IndexEntry.Words = words;
            return true;
        }
    }
}