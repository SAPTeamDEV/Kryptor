using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Extensions;

using static System.Net.Mime.MediaTypeNames;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreAnalyzeRootSession : Session
    {
        private readonly SessionContainer container;
        private byte[] test;

        public Stopwatch CalcTimer { get; }
        public bool Found { get; private set; }

        public KeyStoreAnalyzeRootSession(int maxRunningSessions)
        {
            Progress = -1;

            container = new SessionContainer(maxRunningSessions);

            CalcTimer = new Stopwatch();
        }

        protected override async Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            Description = "Calculating crack time";

            var sample = new byte[3] { 127, 255, 255 };
            test = sample.Sha256();

            CalcTimer.Start();

            for (int i = 0; i < 256; i++)
            {
                var session = new KeyStoreAnalyzeSession(i, test);
                session.OnVerify += StopTimer;
                container.NewSession(session, false);
            }

            await container.WaitAll(cancellationToken);

            return true;
        }

        private void StopTimer()
        {
            CalcTimer.Stop();
            Found = true;

            foreach (var token in container.TokenSources)
            {
                token.Cancel();
            }
        }
    }
}