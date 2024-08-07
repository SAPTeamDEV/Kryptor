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
        bool isFound;
        ManagedSession mSession;

        KeyStore KeyStore { get; set; }
        byte[] Test {  get; set; }
        Stopwatch Timer { get; }

        public KeyStoreAnalyzeSessionHost(bool verbose, string keystore) : base(verbose)
        {
            ks = keystore;

            Timer = new Stopwatch();
        }

        public override void Start()
        {
            base.Start();
            KeyStore = LoadKeyStore(ks);

            var sample = new byte[3] { 127, 255, 255 };
            Test = sample.Sha256();

            mSession = new ManagedSession();
            mSession.SetProgress(-1);
            mSession.SetDescription("Calculating crack time");

            var mHolder = new SessionHolder(mSession, new CancellationTokenSource());
            Container.Add(mHolder);

            Timer.Start();

            for (int i = 0; i < 256; i++)
            {
                var session = new KeyStoreAnalyzeSession(i, Test);
                session.OnVerify += StopTimer;
                NewSession(session, true);
            }

            ShowProgressMonitored(true).Wait();

            double ratio = Math.Pow(2, 32) / Math.Pow(2, 3);
            double estimatedTimeFor32ByteArray = Timer.Elapsed.TotalSeconds * ratio;
            double estimatedTimeForLargeArrayInYears = Math.Round((double)TimeSpan.FromSeconds(estimatedTimeFor32ByteArray).Days / 365 * KeyStore.Keys.Length, 2);

            if (isFound)
            {
                Console.WriteLine($"Estimated crack time with your cpu is ~{estimatedTimeForLargeArrayInYears} years");
            }
            else
            {
                Console.WriteLine("Cannot determine crack time.");
            }
        }

        void StopTimer()
        {
            Timer.Stop();
            isFound = true;
            mSession.SetEndStatus(SessionEndReason.Completed);

            foreach (var token in Container.TokenSources)
            {
                token.Cancel();
            }
        }
    }
}