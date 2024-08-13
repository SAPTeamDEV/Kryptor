using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreRandomLoadSession : KeyStoreLoadSession
    {
        private CancellationToken CancellationToken;

        private KeyStoreGenerator Generator;
        private int Size;

        public KeyStoreRandomLoadSession(KeyStoreGenerator generator, int size, int margin)
        {
            Generator = generator;
            Size = (size * 32) + margin;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            Description = "Generating keystore";

            await Task.Delay(2);
            IGenerator gen;

            switch (Generator)
            {
                case KeyStoreGenerator.CryptoRng:
                    gen = new CryptoRandom();
                    break;
                case KeyStoreGenerator.Unix:
                    gen = new UnixRandom();
                    break;
                case KeyStoreGenerator.SafeRng:
                    gen = new SafeRng();
                    break;
                case KeyStoreGenerator.EntroX:
                    gen = new EntroX();
                    break;
                default:
                    throw new System.ArgumentException("generator");
            }

            gen.OnProgress += UpdateProgress;

            var buffer = new byte[Size];
            gen.Generate(buffer);

            Description = "Loading keystore";
            Progress = -1;
            KeyStore = new KeyStore(buffer);

            Description = "Keystore loaded";
            return true;
        }

        private void UpdateProgress(double progress)
        {
            Progress = progress;
            CancellationToken.ThrowIfCancellationRequested();
        }
    }
}