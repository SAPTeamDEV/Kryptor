using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreAnalyzeRootSession : Session, ISessionHost
    {
        private readonly SessionContainer container;
        private byte[] test;

        public Stopwatch CalcTimer { get; }
        public bool Found { get; private set; }

        public KeyStoreAnalyzeRootSession(int maxRunningSessions)
        {
            Progress = -1;

            container = new SessionContainer(this, maxRunningSessions);

            CalcTimer = new Stopwatch();
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Description = "Calculating crack time";

            byte[] sample = new byte[3] { 127, 255, 255 };
            test = sample.Sha256();

            CalcTimer.Start();

            for (int i = 0; i < 256; i++)
            {
                KeyStoreAnalyzeSession session = new KeyStoreAnalyzeSession(i, test);
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

            foreach (CancellationTokenSource token in container.TokenSources)
            {
                token.Cancel();
            }
        }

        public void Start(ClientContext context) => throw new System.NotImplementedException();
        public void End(bool cancelled) => throw new System.NotImplementedException();
        public void NewSession(ISession session, bool autoRemove) => container.NewSession(session, autoRemove);
        public void MonitorTask(Task task) => container.AddMonitoringTask(task);
    }
}