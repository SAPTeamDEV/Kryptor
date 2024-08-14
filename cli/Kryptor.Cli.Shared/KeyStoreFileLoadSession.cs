using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreFileLoadSession : KeyStoreLoadSession
    {
        private readonly string path;
        private const int ChunckSize = 4096;

        public KeyStoreFileLoadSession(bool showFingerprint, string path) : base(showFingerprint) => this.path = path;

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            using (FileStream f = File.OpenRead(path))
            {
                Description = "Reading Keystore file";

                byte[] result = new byte[f.Length];

                double step = (double)((double)ChunckSize / f.Length) * 100;
                int prog = 1;

                for (int i = 0; i < f.Length; i += ChunckSize)
                {
                    byte[] buffer = new byte[Math.Min(f.Length - f.Position, ChunckSize)];
                    await f.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    Array.Copy(buffer, 0, result, i, buffer.Length);

                    Progress = step * prog;
                    prog++;
                }

                Description = "Loading keystore";
                Progress = -1;
                KeyStore = new KeyStore(result);

                SetEndDescription();
                return true;
            }
        }
    }
}