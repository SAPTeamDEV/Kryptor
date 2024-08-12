using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;

#if !NETFRAMEWORK
using ANSIConsole;
#endif

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliSessionHost : SessionHost
    {
        public bool Verbose { get; }

        public CliSessionHost(bool verbose)
        {
            Verbose = verbose;
            Program.Context.CatchExceptions = !Verbose;
        }

        public override void Start(ClientContext context)
        {
            Log($"Kryptor Command-Line Interface v{Program.Context.CliVersion.Color(Color.Cyan)}");
            Log($"Engine version: {Program.Context.EngineVersion.Color(Color.Cyan)}");
        }

        protected void Log(string message = null) => Console.WriteLine(message);

        protected void DebugLog(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
                File.AppendAllText("debug.txt", DateTime.Now.ToString() + " - " + message + "\r\n");
            }
        }

        protected async Task ShowProgress(bool showOverall, bool showRemaining = true)
        {
            bool isRedirected = Console.IsOutputRedirected;
            int bufferWidth = Console.BufferWidth;
            int paddingBufferSize = isRedirected ? 1 : bufferWidth;

            Stopwatch sw = null;
            int qCounter = 0;

            List<string> loadingSteps = new List<string>()
            {
                "||--",
                "|||-",
                "-|||",
                "--||",
                "---|",
                "--||",
                "-|||",
                "|||-",
                "||--",
                "|---",
            };
            int loadingStep = 0;

            List<string> waitingSteps = new List<string>()
            {
                "|---",
                "||--",
                "|||-",
                "||||",
                "-|||",
                "--||",
                "---|",
                "----",
                "----",
                "----",
                "----",
            };
            int waitingStep = 0;

            if (!isRedirected)
            {
                Console.CursorVisible = false;
            }

            SessionHolder[] holders = Container.Holders.ToArray();
            List<ISession> sessions = holders.Select(x => x.Session).ToList();

            if (sessions.Count == 0 || (!isRedirected && bufferWidth < 50))
            {
                showOverall = false;
            }

            int extraLines = 0;

            if (showOverall)
            {
                sw = Stopwatch.StartNew();
                extraLines++;
            }

            IEnumerable<ISession> blockingSessions = sessions.Where(x => x.Status != SessionStatus.Managed);
            List<ISession> flaggedSessions = new List<ISession>();

            int lines = Container.Sessions.Where(x => !x.IsHidden).Count() + extraLines;
            int maxLines = Console.BufferHeight - 1;
            int ceilingLine = 0;

            while (true)
            {
                bool isCompleted = blockingSessions.All(x => x.Status == SessionStatus.Ended);
                int curLines = extraLines;

                double totalProg = 0;
                int count = 0;

                double runningProg = 0;
                double runningRem = 0;
                int runningCount = 0;

                foreach (ISession session in sessions)
                {
                    if (session.Progress >= 0
                        && (session.IsRunning
                            || session.Status == SessionStatus.NotStarted
                            || (session.Status == SessionStatus.Ended && (session.EndReason == SessionEndReason.Completed
                                                                          || session.EndReason == SessionEndReason.Cancelled))))
                    {
                        double sProg = session.Progress;

                        totalProg += sProg;
                        count++;

                        if (session.IsRunning && session.Timer != null)
                        {
                            runningProg += sProg;
                            runningRem += Utilities.CalculateRemainingTime(sProg, session.Timer.ElapsedMilliseconds);
                            runningCount++;
                        }
                    }

                    if (!session.IsHidden && (!isRedirected || session.Status == SessionStatus.Ended))
                    {
                        if (isRedirected)
                        {
                            flaggedSessions.Add(session);
                        }
                        else
                        {
                            if (!isCompleted && lines > maxLines)
                            {
                                if (session.Status != SessionStatus.Running || curLines >= maxLines)
                                {
                                    continue;
                                }
                            }
                        }

                        GetSessionInfo(isRedirected, bufferWidth, loadingSteps, loadingStep, waitingSteps, waitingStep, session, out Color color, out string prog, out string desc);

                        Console.WriteLine($"[{prog.Color(color)}] {desc}".PadRight(paddingBufferSize));

                        curLines++;
                    }
                };

                foreach (ISession session in flaggedSessions)
                {
                    sessions.Remove(session);
                }

                flaggedSessions.Clear();

                if (showOverall && (!isRedirected || isCompleted))
                {
                    totalProg = count > 0 ? Math.Round(totalProg / count, 2) : 0;
                    runningRem = runningCount > 0 ? runningRem / runningCount : 0;
                    TimeSpan elapsedTime = sw.Elapsed;
                    TimeSpan remainingTime = TimeSpan.FromMilliseconds(runningRem);

                    string ovText = "";

                    if (totalProg > 0)
                    {
                        ovText = $"[{totalProg}%] ";
                    }

                    ovText += $"Elapsed: {elapsedTime:hh\\:mm\\:ss}";

                    if (showRemaining && runningRem > 0)
                    {
                        ovText += $" Remaining: {remainingTime:hh\\:mm\\:ss}";
                    }

                    Console.WriteLine(ovText.PadRight(paddingBufferSize));
                }

                if (curLines > ceilingLine)
                {
                    ceilingLine = curLines;
                }
                else if (curLines < ceilingLine)
                {
                    while (curLines < ceilingLine)
                    {
                        Console.WriteLine("".PadRight(paddingBufferSize));
                        curLines++;
                    }
                }

                loadingStep = (++loadingStep) % loadingSteps.Count;
                waitingStep = (++waitingStep) % waitingSteps.Count;

                if (isCompleted)
                {
                    sw?.Stop();

                    if (!isRedirected)
                    {
                        Console.CursorVisible = true;
                    }

                    IEnumerable<ISession> _sessions = holders.Select(x => x.Session);
                    foreach (SessionHolder holder in holders.Where(x => x.Session.Messages.Count > 0))
                    {
                        bool showId = _sessions.Where(x => x.GetType().IsAssignableFrom(holder.Session.GetType())).Count() > 1;
                        string prefix = $"{holder.Session.GetType().Name}";
                        if (showId)
                        {
                            prefix += $"({holder.Id})";
                        }

                        foreach (string message in holder.Session.Messages)
                        {
                            Log($"{prefix} -> {message}");
                        }
                    }

                    break;
                }

                if (MasterToken.IsCancellationRequested && qCounter++ % 10 == 6)
                {
                    Container.StartQueuedSessions();
                }

                if (!isRedirected)
                {
                    await Task.Delay(100);

                    Console.CursorLeft = 0;
                    Console.CursorTop -= Math.Min(ceilingLine, maxLines);
                }
            }
        }

        private static void GetSessionInfo(bool isRedirected, int bufferWidth, List<string> loadingSteps, int loadingStep, List<string> waitingSteps, int waitingStep, ISession session, out Color color, out string prog, out string desc)
        {
            color = Color.DarkCyan;
            prog = waitingSteps[waitingStep];

            if (session.IsRunning)
            {
                color = Color.Yellow;
                prog = session.Progress < 0 || session.Progress > 100.0 ? loadingSteps[loadingStep] : $"{Math.Round(session.Progress, 2)}%".PadRight(6);
            }
            else if (session.Status == SessionStatus.Ended)
            {
                switch (session.EndReason)
                {
                    case SessionEndReason.Completed:
                        color = Color.LawnGreen;
                        prog = "done";
                        break;
                    case SessionEndReason.Failed:
                        color = Color.Red;
                        prog = "error";
                        break;
                    case SessionEndReason.Cancelled:
                        color = Color.Orange;
                        prog = "cancelled";
                        break;
                    case SessionEndReason.Skipped:
                        color = Color.Silver;
                        prog = "skipped";
                        break;
                    default:
                        color = Color.DarkRed;
                        prog = "unknown";
                        break;
                }
            }

            desc = session.Description;
            int expectedLength = bufferWidth - prog.Length - 5;

            if (!isRedirected && desc.Length > expectedLength)
            {
                desc = $"...{desc.Substring(desc.Length - expectedLength + 3)}";
            }
        }

        protected Task ShowProgressMonitored(bool showOverall, bool showRemaining = true)
        {
            Task pTask = ShowProgress(showOverall, showRemaining);
            MonitorTask(pTask);
            return pTask;
        }

        protected KeyStore LoadKeyStore(string keyStore)
        {
            KeyStore ks;

            KeyStoreLoadSession session = CreateKeyStoreLoadSession(keyStore);
            NewSession(session, true);

            ShowProgressMonitored(false).Wait();
            ks = session.KeyStore;

            if (session.EndReason != SessionEndReason.Completed)
            {
                throw new ApplicationException("KeyStoreLoad failed");
            }

            DebugLog($"Keystore fingerprint: {ks.Fingerprint.FormatFingerprint()}");

            return ks;
        }

        protected KeyStoreLoadSession CreateKeyStoreLoadSession(string keyStore)
        {
            KeyStoreLoadSession session;
            if (File.Exists(keyStore))
            {
                DebugLog($"Keystore file: {keyStore}");
                session = new KeyStoreFileLoadSession(keyStore);

            }
            else if (TransformerToken.IsValid(keyStore))
            {
                DebugLog($"Transformer token: {keyStore}");
                TransformerToken token = TransformerToken.Parse(keyStore);

                if (Verbose)
                {
                    ITranformer tranformer = Transformers.GetTranformer(token);
                    DebugLog($"Generating keystore with {token.KeySize} keys using {tranformer.GetType().Name}");
                }

                session = new KeyStoreTokenLoadSession(token);
            }
            else
            {
                throw new FileNotFoundException(keyStore);
            }

            return session;
        }
    }
}