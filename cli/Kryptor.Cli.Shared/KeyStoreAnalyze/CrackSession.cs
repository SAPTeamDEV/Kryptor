using System.Diagnostics;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli.KeyStoreAnalyze
{
    public class CrackSession : Session, ISessionHost
    {
        private readonly SessionContainer container;
        private byte[] test;

        public Stopwatch CalculationTimer { get; }
        public bool Found { get; private set; }
        public bool Verbose => false;

        public CrackSession(int maxRunningSessions)
        {
            Progress = -1;

            container = new SessionContainer(this, maxRunningSessions);

            CalculationTimer = new Stopwatch();
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Description = "Calculating crack time";

            byte[] sample = new byte[3] { 127, 255, 255 };
            test = sample.Sha256();

            CalculationTimer.Start();

            for (int i = 0; i < 256; i++)
            {
                CrackSubSession session = new CrackSubSession(i, test);
                session.OnVerify += StopTimer;
                NewSession(session, false, true);
            }

            await container.WaitAll(cancellationToken);

            return true;
        }

        private void StopTimer()
        {
            CalculationTimer.Stop();
            Found = true;

            foreach (CancellationTokenSource token in container.TokenSources)
            {
                token.Cancel();
            }
        }

        public void Start(ClientContext context) => throw new System.NotImplementedException();

        public void End(bool canceled) => throw new System.NotImplementedException();

        public void NewSession(ISession session, bool autoRemove, bool autoStart) => container.NewSession(session, autoRemove, autoStart);

        public void MonitorTask(Task task) => throw new System.NotImplementedException();

        public Task<TResponse> OnSessionRequest<TResponse>(ISession session, SessionRequest<TResponse> request, CancellationToken cancellationToken) => throw new System.NotImplementedException();
    }
}