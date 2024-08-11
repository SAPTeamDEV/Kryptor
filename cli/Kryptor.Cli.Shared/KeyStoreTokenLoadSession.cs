using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreTokenLoadSession : KeyStoreLoadSession
    {
        TransformerToken token;
        CancellationToken cancellationToken;

        const int ChunckSize = 4096;

        public KeyStoreTokenLoadSession(TransformerToken token)
        {
            this.token = token;
        }

        protected async override Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            Description = "Generating keystore";
            await Task.Delay(1);

            KeyStore = Utilities.GenerateKeyStoreFromToken(token, UpdateProgress);

            Description = "Keystore loaded";
            return true;
        }

        void UpdateProgress(double progress)
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