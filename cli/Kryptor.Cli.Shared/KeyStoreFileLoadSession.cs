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
            return false;
            /*
            using (var f = File.OpenRead(path))
            {
                var result = new byte[f.Length];

                var step = f.Length / (double)ChunckSize;
                double prog = 0;

                for (int i = 0; i < f.Length; i += ChunckSize)
                {
                    var buffer = new byte[ChunckSize];
                    f.ReadAsync(buffer, )
                }
            }
            */
        }
    }
}