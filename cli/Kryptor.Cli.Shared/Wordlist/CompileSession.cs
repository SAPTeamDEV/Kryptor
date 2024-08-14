using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MoreLinq;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli.Wordlist
{
    public class CompileSession : Session
    {
        private bool cleaned;
        private readonly Dictionary<string, FileStream> fileStreams = new Dictionary<string, FileStream>();

        public string FilePath;
        public string DestPath;

        public WordlistIndexEntryV2 IndexEntry;
        private readonly bool Indexing;
        private readonly bool Importing;

        private bool Bypass => Indexing || Importing;

        public CompileSession(string path, string destination, WordlistIndexEntryV2 entry, bool indexing, bool importing)
        {
            if (indexing || !importing)
            {
                Description = "Waiting for download";
            }

            FilePath = path;
            DestPath = destination;

            IndexEntry = entry;

            Indexing = indexing;
            Importing = importing;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            try
            {
                InstallSessionHost installer = PreCheck(sessionHost);

                Description = $"Importing {IndexEntry.Id}";

                await Compile(cancellationToken);

                Progress = 100;
                Description = $"Imported {IndexEntry.Id} wordlist";

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

        private void Cleanup(bool deleteInstallation)
        {
            if (cleaned) return;
            cleaned = true;

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
                await VerifyHash(streamReader, cancellationToken);

                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

                double steps = 1.0 / streamReader.BaseStream.Length * 100;
                IndexEntry.Size = streamReader.BaseStream.Length;

                int readChars = 0;
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    readChars += line.Length;
                    lines++;

                    if (string.IsNullOrEmpty(line) || line.Length < 4) continue;

                    string c = WordlistFragmentCollection.GetWordIdentifier(line).ToString();
                    words++;

                    if (!fileStreams.ContainsKey(c))
                    {
                        fileStreams[c] = File.OpenWrite(Path.Combine(DestPath, c));
                    }

                    byte[] data = Encoding.UTF8.GetBytes(line + "\n");

                    fileStreams[c].Write(data, 0, data.Length);

                    Progress = readChars * steps;
                }
            }

            IndexEntry.Words = words;
            IndexEntry.Lines = lines;
        }

        private InstallSessionHost PreCheck(ISessionHost sessionHost)
        {
            InstallSessionHost installSessionHost = sessionHost is InstallSessionHost ish
                ? ish
                : throw new InvalidOperationException("This session started from an unknown session host. you may start this session only via InstallSessionHost");
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

        private async Task VerifyHash(StreamReader streamReader, CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[streamReader.BaseStream.Length];
                await streamReader.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                byte[] hash = buffer.Sha256();

                if (Bypass && IndexEntry.Hash == null)
                {
                    IndexEntry.Hash = hash;
                }
                else if (IndexEntry.Hash != null)
                {
                    if (!IndexEntry.Hash.SequenceEqual(hash))
                    {
                        throw new InvalidDataException("File is corrupted");
                    }
                }
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch
            {
                // Ignore hash errors
            }
        }
    }
}