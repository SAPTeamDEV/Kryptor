using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreFileLoadSession : Session
    {
        public override double Progress { get; protected set; }
        public override string Description { get; protected set; }

        string path;

        public KeyStore KeyStore { get; protected set; }

        const int ChunckSize = 4096;

        public KeyStoreFileLoadSession(string path)
        {
            this.path = path;
        }

        protected async override Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            using (var f = File.OpenRead(path))
            {
                Description = "Reading Keystore file";

                var result = new byte[f.Length];

                var step = (double)((double)ChunckSize / f.Length) * 100;
                int prog = 1;

                for (int i = 0; i < f.Length; i += ChunckSize)
                {
                    var buffer = new byte[Math.Min(f.Length - f.Position, ChunckSize)];
                    await f.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    Array.Copy(buffer, 0, result, i, buffer.Length);

                    Progress = step * prog;
                    prog++;
                }

                Description = "Loading keystore";
                Progress = -1;
                KeyStore = new KeyStore(result);

                Description = "Keystore loaded";
                return true;
            }
        }
    }
}