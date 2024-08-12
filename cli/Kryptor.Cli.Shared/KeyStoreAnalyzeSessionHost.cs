using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Extensions;

using static System.Net.Mime.MediaTypeNames;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreAnalyzeSessionHost : CliSessionHost
    {
        private readonly string ks;
        private readonly int maxRunningSessions;

        private KeyStore KeyStore { get; set; }

        public KeyStoreAnalyzeSessionHost(bool verbose, int jobs, string keystore) : base(verbose)
        {
            ks = keystore;
            maxRunningSessions = jobs > 0 ? jobs : MaxRunningSessions - 2;
        }

        public override void Start()
        {
            base.Start();

            var ksLoadSession = CreateKeyStoreLoadSession(ks);
            NewSession(ksLoadSession);

            var calcSession = new KeyStoreAnalyzeRootSession(maxRunningSessions);
            calcSession.Dependencies.Add(ksLoadSession);
            NewSession(calcSession);

            ShowProgressMonitored(true).Wait();

            if (!Container.Sessions.All(x => x.EndReason == SessionEndReason.Completed))
            {
                return;
            }

            KeyStore = ksLoadSession.KeyStore;

            double ratio = Math.Pow(2, 32) / Math.Pow(2, 3);
            double estimatedTimeFor32ByteArray = calcSession.CalcTimer.Elapsed.TotalSeconds * ratio;
            double estimatedTimeForLargeArrayInYears = Math.Round((double)TimeSpan.FromSeconds(estimatedTimeFor32ByteArray).Days / 365 * KeyStore.Keys.Length, 2);

            if (calcSession.Found)
            {
                Console.WriteLine($"Estimated crack time with your cpu is ~{estimatedTimeForLargeArrayInYears} years");
            }
            else
            {
                Console.WriteLine("Cannot determine crack time.");
            }
        }
    }
}