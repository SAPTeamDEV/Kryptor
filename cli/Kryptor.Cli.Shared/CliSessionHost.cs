using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliSessionHost : Client.SessionHost
    {
        public bool Quiet { get; }
        public bool NoColor { get; }

        private MemoryStream mem;
        private ConsoleKeyInfo cKey;

        protected ConsoleKeyInfo KeyQueue
        {
            get
            {
                var key = cKey;
                cKey = default;
                return key;
            }

            set
            {
                cKey = value;
            }
        }

        public CliSessionHost(GlobalOptions globalOptions)
        {
            Verbose = globalOptions.Verbose;
            Quiet = globalOptions.Quiet;
            NoColor = globalOptions.NoColor;
        }

        public override void Start(ClientContext context)
        {
            CliContext cliContext = context as CliContext;
            cliContext.CatchExceptions = !Verbose;
            cliContext.NoColor = NoColor;

            if (Quiet)
            {
                StreamWriter tw = new StreamWriter(mem = new MemoryStream());
                Console.SetOut(tw);
            }

            Log($"Kryptor Command-Line Interface v{Program.Context.CliVersion.WithColor(Color.Cyan)}");
            Log($"Engine version: {Program.Context.EngineVersion.WithColor(Color.Cyan)}");
        }

        protected void Log(string message = null) => Console.WriteLine(message);
        protected void LogError(string message = null) => Console.Error.WriteLine(message);

        protected void DebugLog(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
#if DEBUG
                File.AppendAllText("debug.txt", DateTime.Now.ToString() + " - " + message + "\r\n");
#endif
            }
        }

        private async Task ShowProgressImpl(bool showOverall, bool showRemaining = true)
        {
            bool isRedirected = Console.IsOutputRedirected || Quiet;
            int bufferWidth = Console.BufferWidth;
            int paddingBufferSize = isRedirected ? 1 : bufferWidth;

            Stopwatch sw = null;
            int qCounter = 0;

            string[] loadingSteps = new string[]
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

            string[] waitingSteps = new string[]
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

            string[] pauseSteps = new string[]
            {
                "||||",
                "||||",
                "----",
                "----",
                "----",
                "----",
            };
            int pauseStep = 0;

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

                string curLoadingStep = loadingSteps[loadingStep];
                string curWaitingStep = waitingSteps[waitingStep];
                string curPauseStep = pauseSteps[pauseStep];

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
                                if (!session.IsRunning || curLines >= maxLines)
                                {
                                    continue;
                                }
                            }
                        }

                        GetSessionInfo(isRedirected, bufferWidth, curLoadingStep, curWaitingStep, curPauseStep, session, out Color color, out string prog, out string desc);

                        Console.Write($"[{prog.WithColor(color)}] {desc}");

                        int padLength = paddingBufferSize - prog.Length - 3 - desc.Length;
                        Console.WriteLine("".PadRight(padLength > -1 ? padLength : 0));

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
                    ShowOverallTime(showRemaining, paddingBufferSize, sw, ref totalProg, count, ref runningRem, runningCount);
                }

                if (!isRedirected)
                {
                    FillToCeiling(paddingBufferSize, ref ceilingLine, ref curLines);

                    var doListen = !Request.IsEmpty() && !Request.IsResponsed;

                    if (doListen)
                    {
                        Console.Write(Request.Message + " (Y/n)");
                    }

                    var key = KeyQueue;
                    if (key != default)
                    {
                        if (doListen && (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N))
                        {
                            Request.SetResponse(key.Key == ConsoleKey.Y);
                        }
                    }
                }

                loadingStep = (++loadingStep) % loadingSteps.Length;
                waitingStep = (++waitingStep) % waitingSteps.Length;
                pauseStep = (++pauseStep) % pauseSteps.Length;

                if (isCompleted)
                {
                    sw?.Stop();

                    if (!isRedirected)
                    {
                        Console.CursorVisible = true;
                    }

                    PrintMessages(holders);

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

        private static void FillToCeiling(int paddingBufferSize, ref int ceilingLine, ref int curLines)
        {
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

            Console.Write("".PadRight(paddingBufferSize));
            Console.CursorLeft = 0;
        }

        private void PrintMessages(SessionHolder[] holders)
        {
            IEnumerable<ISession> _sessions = holders.Select(x => x.Session);
            foreach (SessionHolder holder in holders.Where(x => x.Session.Messages.Count > 0))
            {
                bool showId = _sessions.Where(x => x.GetType().IsAssignableFrom(holder.Session.GetType())).Count() > 1;
                string prefix = $"{holder.Session.Name}";
                if (showId)
                {
                    prefix += $"({holder.Id})";
                }

                foreach (string message in holder.Session.Messages)
                {
                    LogError($"{prefix} -> {message}");
                }
            }
        }

        private static void ShowOverallTime(bool showRemaining, int paddingBufferSize, Stopwatch sw, ref double totalProg, int count, ref double runningRem, int runningCount)
        {
            totalProg = count > 0 ? Math.Round(totalProg / count, 2) : 0;
            runningRem = runningCount > 0 ? runningRem / runningCount : 0;
            TimeSpan elapsedTime = sw.Elapsed;
            TimeSpan remainingTime;

            try
            {
                remainingTime = TimeSpan.FromMilliseconds(runningRem);
            }
            catch
            {
                remainingTime = default;
            }

            string ovText = "";

            if (totalProg > 0)
            {
                ovText = $"[{totalProg}%] ";
            }

            ovText += $"Elapsed: {elapsedTime:hh\\:mm\\:ss}";

            if (showRemaining && runningRem > 0 && remainingTime != default)
            {
                ovText += $" Remaining: {remainingTime:hh\\:mm\\:ss}";
            }

            Console.WriteLine(ovText.PadRight(paddingBufferSize));
        }

        private static void GetSessionInfo(bool isRedirected, int bufferWidth, string loading, string waiting, string pause, ISession session, out Color color, out string prog, out string desc)
        {
            color = Color.DarkCyan;
            prog = waiting;

            if (session.IsRunning)
            {
                color = Color.Yellow;
                prog = session.IsPaused ? pause : session.Progress <= 0 || session.Progress > 100.00 ? loading : $"{Math.Round(session.Progress, 2)}%".PadBoth(6);
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

        private async Task ReadKey()
        {
            await Task.Delay(2);

            while (true)
            {
                KeyQueue = Console.ReadKey(true);
            }
        }

        protected Task ShowProgress(bool showOverall, bool showRemaining)
        {
            Task pTask = ShowProgressImpl(showOverall, showRemaining);

            _ = ReadKey();
            return pTask;
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

            return session.EndReason != SessionEndReason.Completed ? throw new ApplicationException("KeyStoreLoad failed") : ks;
        }

        protected KeyStoreLoadSession CreateKeyStoreLoadSession(string keyStore)
        {
            KeyStoreLoadSession session;
            if (File.Exists(keyStore))
            {
                DebugLog($"Keystore file: {keyStore}");
                session = new KeyStoreFileLoadSession(Verbose, keyStore);

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

                session = new KeyStoreTokenLoadSession(Verbose, token, 0);
            }
            else
            {
                throw new FileNotFoundException(keyStore);
            }

            return session;
        }

        public PauseRequest Request { get; private set; } = new PauseRequest(null);

        public override bool OnSessionPaused(ISession session, string message)
        {
            Request = new PauseRequest(message);

            while (!Request.IsResponsed)
            {
                Thread.Sleep(5);
            }

            return Request.Response;
        }
    }
}