using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreTokenLoadSession : KeyStoreLoadSession
    {
        private TransformerToken token;
        private readonly int margin;

        private CancellationToken cancellationToken;

        public KeyStoreTokenLoadSession(bool showFingerprint, TransformerToken token, int margin) : base(showFingerprint)
        {
            this.token = token;
            this.margin = margin;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            Description = "Generating keystore";
            await Task.Delay(1);

            KeyStore = Utilities.GenerateKeyStoreFromToken(token, UpdateProgress, margin);

            SetEndDescription();
            return true;
        }

        private void UpdateProgress(object sender, double progress)
        {
            if (progress == -1)
            {
                Description = "Loading keystore";
            }

            Progress = progress;
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}