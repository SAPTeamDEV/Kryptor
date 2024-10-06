using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli.KeyStoreAnalyze
{
    public sealed class SessionHost : CliSessionHost
    {
        private readonly string ks;
        private readonly int maxRunningSessions;

        private KeyStore KeyStore { get; set; }

        public SessionHost(GlobalOptions globalOptions, int jobs, string keystore) : base(globalOptions)
        {
            ks = keystore;
            maxRunningSessions = jobs > 0 ? jobs : Container.MaxRunningSessions - 2;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStoreLoadSession ksLoadSession = CreateKeyStoreLoadSession(ks);
            NewSession(ksLoadSession);

            CrackSession calcSession = new CrackSession(maxRunningSessions);
            ksLoadSession.ContinueWith(calcSession);
            NewSession(calcSession);

            ShowProgressMonitored(true).Wait();

            if (!Container.Sessions.All(x => x.EndReason == SessionEndReason.Completed))
            {
                return;
            }

            KeyStore = ksLoadSession.KeyStore;

            double ratio = Math.Pow(2, 32) / Math.Pow(2, 3);
            double estimatedTimeFor32ByteArray = calcSession.CalculationTimer.Elapsed.TotalSeconds * ratio;
            double estimatedTimeForLargeArrayInYears = (double)TimeSpan.FromSeconds(estimatedTimeFor32ByteArray).Days / 365 * KeyStore.Keys.Length;

            if (calcSession.Found)
            {
                Console.WriteLine($"Estimated crack time with your cpu is ~{estimatedTimeForLargeArrayInYears.FormatWithCommas()} years");
            }
            else
            {
                Console.WriteLine("Cannot determine crack time.");
            }
        }
    }
}