using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreTokenLoadSession : Session
    {
        public override double Progress { get; protected set; }
        public override string Description { get; protected set; }

        TransformerToken token;
        CancellationToken cancellationToken;

        public KeyStore KeyStore { get; protected set; }

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