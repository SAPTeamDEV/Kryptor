using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using MoreLinq;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class CompileSession : Session
    {
        private bool cleaned;
        private readonly Dictionary<int, FileStream> fileStreams = new Dictionary<int, FileStream>();
        private Regex regex = new Regex(@"[^\x20-\x7E]");
        private HashSet<string> uniqueLines = new HashSet<string>();

        public string FilePath;
        public string DestPath;

        public WordlistIndexEntryV2 IndexEntry;
        private readonly bool Indexing;
        private readonly bool Importing;
        private readonly bool Optimize;

        public CompileSession(string path, string destination, WordlistIndexEntryV2 entry, bool optimize, bool indexing, bool importing)
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

        private bool TryAddLine(string line)
        {
            return !Optimize || uniqueLines.Add(line);
        }

        private void Cleanup(bool deleteInstallation)
        {
            if (cleaned) return;
            cleaned = true;

            if (uniqueLines != null)
            {
                uniqueLines.Clear();
                uniqueLines = null;
            }

            foreach (FileStream f in fileStreams.Values)
            {
                f.Flush();
                f.Dispose();
            }

            Dependencies.OfType<DownloadSession>().ForEach(x => x.DeleteCache());

            if (deleteInstallation && Directory.Exists(DestPath))
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

                    int c = WordlistFragmentCollection.GetWordIdentifier(line);
                    if (!fileStreams.ContainsKey(c))
                    {
                        fileStreams[c] = File.OpenWrite(Path.Combine(DestPath, c.ToString()));
                    }

                    fileStreams[c].Write(data, 0, data.Length);
                }
            }

            IndexEntry.Words = words;
            IndexEntry.Lines = lines;
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