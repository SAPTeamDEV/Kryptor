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

#if false
            if (!await SendRequest(sessionHost, new SessionRequest<bool>("Do you want to continue this task? even when you see this long message? tou know, i don't want to continue generating these keystores, i have a family and two kids to feed, please free me. i know that you may need this keystore to encrypt or decrypt your highly secret files, you are the boss.", true), cancellationToken))
            {
                throw new System.OperationCanceledException();
            }
#endif

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