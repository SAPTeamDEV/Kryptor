using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using MoreLinq;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class CompileSession : Session
    {
        private bool cleaned;
        private Dictionary<int, FileStream> fileStreams = new Dictionary<int, FileStream>();
        private Dictionary<int, string> lookupStrings = new Dictionary<int, string>();
        private readonly Regex regex = new Regex(@"[^\x20-\x7E]");
        private HashSet<string> uniqueLines = new HashSet<string>();

        public string FilePath;
        public string DestPath;

        public WordlistIndexEntry IndexEntry;
        private readonly bool Indexing;
        private readonly bool Importing;
        private readonly bool Optimize;

        public CompileSession(string path, string destination, WordlistIndexEntry entry, bool optimize, bool indexing, bool importing)
        {
            if (indexing || !importing)
            {
                Description = $"{entry.Id}: Waiting for download";
            }

            FilePath = path;
            DestPath = destination;

            IndexEntry = entry;

            Indexing = indexing;
            Importing = importing;

            Optimize = optimize;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            try
            {
                SessionHost installer = PreCheck(sessionHost);

                Description = $"Importing {IndexEntry.Id}";

                await Compile(cancellationToken);

                Progress = 100;
                Description = $"Imported {IndexEntry.Id} wordlist";

                IndexEntry.Optimized = Optimize;
                if (!Indexing)
                {
                    IndexEntry.InstallDirectory = DestPath;

                    if (fileStreams.Count > 0 && lookupStrings.Count > 0 && fileStreams.Count == lookupStrings.Count)
                    {
                        Description = "Adding verification metadata";
                        AddVerificationMetadata();
                    }
                }

                installer.FinalizeInstallation(IndexEntry);

                Description = Indexing ? $"{IndexEntry.Id} Indexed" : $"{IndexEntry.Id} Installed";
            }
            catch
            {
                Cleanup(true);
                throw;
            }
            finally
            {
                Cleanup(false);
            }

            return true;
        }

        private bool TryAddLine(string line) => !Optimize || uniqueLines.Add(line);

        private void Cleanup(bool deleteInstallation)
        {
            if (cleaned) return;
            cleaned = true;

            uniqueLines.Clear();
            uniqueLines = null;

            foreach (FileStream f in fileStreams.Values)
            {
                f.Flush();
                f.Dispose();
            }

            fileStreams.Clear();
            fileStreams = null;

            lookupStrings.Clear();
            lookupStrings = null;

            Dependencies.OfType<DownloadSession>().ForEach(x => x.DeleteCache());

            if (Directory.Exists(DestPath) && (deleteInstallation || Directory.GetFiles(DestPath).Length == 0))
            {
                Directory.Delete(DestPath, true);
            }
        }

        private async Task Compile(CancellationToken cancellationToken)
        {
            long lines = 0;
            long words = 0;

            using (StreamReader streamReader = new StreamReader(FilePath, Encoding.UTF8))
            {
                double steps = 1.0 / streamReader.BaseStream.Length * 100;

                long readChars = 0;
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    readChars += line.Length;
                    lines++;

                    Progress = readChars * steps;

                    line = line.Trim();

                    if (string.IsNullOrEmpty(line) || line.Length < 4 || !TryAddLine(line) || regex.IsMatch(line)) continue;

                    byte[] data = Encoding.UTF8.GetBytes(line + "\n");
                    words++;

                    if (!Indexing)
                    {
                        int c = WordlistFragmentCollection.GetWordIdentifier(line);
                        if (!fileStreams.ContainsKey(c))
                        {
                            lookupStrings[c] = line;
                            fileStreams[c] = File.Open(Path.Combine(DestPath, c.ToString()), FileMode.Create, FileAccess.ReadWrite);
                        }

                        fileStreams[c].Write(data, 0, data.Length);
                    }
                }
            }

            IndexEntry.Words = words;
            IndexEntry.Lines = lines;
        }

        private void AddVerificationMetadata()
        {
            List<WordlistVerificationMetadata> metadata = new List<WordlistVerificationMetadata>();

            foreach (KeyValuePair<int, FileStream> f in fileStreams)
            {
                metadata.Add(new WordlistVerificationMetadata()
                {
                    FragmentId = f.Key,
                    LookupString = lookupStrings[f.Key],
                    Checksum = Utilities.XOR(IndexEntry.Hash, f.Value.Sha256())
                });
            }

            string mJson = JsonConvert.SerializeObject(metadata);
            byte[] mEncode = Encoding.UTF8.GetBytes(mJson);

            File.WriteAllBytes(Path.Combine(IndexEntry.InstallDirectory, "metadata.json"), mEncode);
        }

        private SessionHost PreCheck(ISessionHost sessionHost)
        {
            SessionHost installSessionHost = sessionHost is SessionHost ish
                ? ish
                : throw new InvalidOperationException("This session started from an unknown session host. you may start this session only via wordlists's SessionHost");
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException(FilePath);
            }

            if (Directory.Exists(DestPath))
            {
                Directory.Delete(DestPath, true);
            }

            Directory.CreateDirectory(DestPath);

            return installSessionHost;
        }
    }
}