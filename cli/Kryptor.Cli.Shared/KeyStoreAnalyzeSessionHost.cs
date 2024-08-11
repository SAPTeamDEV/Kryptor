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
        string ks;

        KeyStore KeyStore { get; set; }

        public KeyStoreAnalyzeSessionHost(bool verbose, string keystore) : base(verbose)
        {
            ks = keystore;
        }

        public override void Start()
        {
            base.Start();

            var ksLoadSession = CreateKeyStoreLoadSession(ks);
            NewSession(ksLoadSession);

            var calcSession = new KeyStoreAnalyzeRootSession(MaxRunningSessions);
            calcSession.SessionDependencies.Add(ksLoadSession);
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