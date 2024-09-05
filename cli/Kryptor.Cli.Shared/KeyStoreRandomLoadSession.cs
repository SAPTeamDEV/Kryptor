using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Generators;
using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreRandomLoadSession : KeyStoreLoadSession
    {
        private CancellationToken CancellationToken;

        private readonly KeyStoreGenerator Generator;
        private readonly int Size;

        public KeyStoreRandomLoadSession(bool showFingerprint, KeyStoreGenerator generator, int size, int margin) : base(showFingerprint)
        {
            Generator = generator;
            Size = (size * 32) + margin;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            Description = "Generating keystore";

            await AsyncCompat.Delay(2, cancellationToken);
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

            gen.ProgressChanged += UpdateProgress;

            byte[] buffer = new byte[Size];
            gen.Generate(buffer);

            Description = "Loading keystore";
            Progress = -1;
            KeyStore = new KeyStore(buffer);

            SetEndDescription();
            return true;
        }

        private void UpdateProgress(object sender, double progress)
        {
            Progress = progress;
            CancellationToken.ThrowIfCancellationRequested();
        }
    }
}