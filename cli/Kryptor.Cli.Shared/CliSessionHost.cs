using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliSessionHost : SessionHost
    {
        public bool Quiet { get; }

        public bool NoColor { get; }

        public bool NoInteractions { get; }

        public bool IsOutputRedirected { get; }

        public PauseRequest Request { get; private set; } = new PauseRequest(null, false);

        public bool HasRequest => !Request.IsEmpty() && !Request.IsResponsed;

        private readonly object _requestLock = new object();
        private MemoryStream mem;
        private ConsoleKeyInfo cKey;
        private bool _readerRunning;

        protected ConsoleKeyInfo KeyQueue
        {
            get
            {
                ConsoleKeyInfo key = cKey;
                cKey = default;
                return key;
            }

            set => cKey = value;
        }

        public CliSessionHost(GlobalOptions globalOptions)
        {
            Verbose = globalOptions.Verbose;
            Quiet = globalOptions.Quiet;
            NoColor = globalOptions.NoColor;

            IsOutputRedirected = Console.IsOutputRedirected || Quiet;
            NoInteractions = IsOutputRedirected || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KRYPTOR_NO_INTERACTION"));
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

            if (BuildInformation.IsAot) LogError("The AOT builds are in development stage and may have unintended behaviors".WithColor(Color.Yellow));
            if (BuildInformation.Variant == BuildVariant.Legacy) LogError("The Legacy version have poor performance and uses old libraries and APIs, it's highly recommended to use the standard or Native variants".WithColor(Color.Yellow));

            Log($"Kryptor Command-Line Interface v{Program.Context.CliVersion.WithColor(Color.Cyan)}");

            if (BuildInformation.Branch == BuildBranch.Debug) Log($"Engine version: {Program.Context.EngineVersion.WithColor(Color.Cyan)}");
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
            int bufferWidth = Console.BufferWidth;
            int paddingBufferSize = IsOutputRedirected ? 1 : bufferWidth;

            Stopwatch sw = null;
            ConsoleFrameBuffer animations = new ConsoleFrameBuffer();
            int qCounter = 0;

            if (!IsOutputRedirected)
            {
                Console.CursorVisible = false;
            }

            SessionHolder[] holders = Container.Holders.ToArray();
            List<ISession> sessions = holders.Select(x => x.Session).ToList();

            if (sessions.Count == 0 || (!IsOutputRedirected && bufferWidth < 50))
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

                    if (!session.IsHidden && (!IsOutputRedirected || session.Status == SessionStatus.Ended))
                    {
                        if (IsOutputRedirected)
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

                        GetSessionInfo(bufferWidth, animations, session, out Color color, out string prog, out string desc);

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

                if (showOverall && (!IsOutputRedirected || isCompleted))
                {
                    ShowOverallTime(showRemaining, paddingBufferSize, sw, ref totalProg, count, ref runningRem, runningCount, isCompleted);
                }

                if (!IsOutputRedirected)
                {
                    FillToCeiling(paddingBufferSize, ref ceilingLine, ref curLines);
                }

                HandleRequests(bufferWidth, paddingBufferSize);

                animations.Next();

                if (isCompleted)
                {
                    sw?.Stop();

                    if (!IsOutputRedirected)
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

                if (!IsOutputRedirected)
                {
                    await Task.Delay(100);

                    Console.CursorLeft = 0;
                    Console.CursorTop -= Math.Min(ceilingLine, maxLines);
                }
            }
        }

        private void HandleRequests(int bufferWidth, int paddingBufferSize)
        {
            if (!NoInteractions)
            {
                if (HasRequest)
                {
                    string choices = Request.Default ? "Y/n" : "y/N";
                    Console.Write($"{Request.Message.Shrink(bufferWidth - 8)} ({choices})");
                }
                else
                {
                    Console.Write("".PadRight(paddingBufferSize));
                    Console.CursorLeft = 0;
                }

                ConsoleKeyInfo key = KeyQueue;
                if (key != default)
                {
                    if (HasRequest)
                    {
                        if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N)
                        {
                            Request.SetResponse(key.Key == ConsoleKey.Y);
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            Request.SetResponse(Request.Default);
                        }
                    }
                }
            }
            else if (HasRequest)
            {
                Request.SetResponse(Request.Default);
            }
        }

        private async Task ShowProgressNewImpl(SessionGroup sessionGroup)
        {
            var bw = Console.BufferWidth;
            var paddingSize = IsOutputRedirected ? 1 : bw;
            int counter = 0;

            if (!IsOutputRedirected)
            {
                Console.CursorVisible = false;
            }

            while (true)
            {
                var ended = sessionGroup.Status == SessionStatus.Ended;

                string progress;
                Color color;
                if (ended)
                {
                    progress = "done";
                    color = Color.LightGreen;
                }
                else
                {
                    progress = $"{Math.Round(sessionGroup.Progress, 2)}%".PadBoth(6);
                    color = Color.Yellow;
                }

                var description = $"S:{sessionGroup.Status} W:{sessionGroup.Waiting} R:{sessionGroup.Running} E:{sessionGroup.Ended} C:{sessionGroup.Count} IN:{sessionGroup.Timer.Elapsed:hh\\:mm\\:ss}";

                if (!IsOutputRedirected)
                {
                    int expectedLength = bw - progress.Length - 5;
                    description = description.Shrink(expectedLength);
                }

                if (!IsOutputRedirected || ended)
                {
                    Console.Write($"[{progress.WithColor(color)}] {description}");

                    int padLength = paddingSize - progress.Length - 3 - description.Length;
                    Console.WriteLine("".PadRight(padLength > -1 ? padLength : 0));
                }

                HandleRequests(bw, paddingSize);

                if (ended)
                {
                    foreach (var msg in sessionGroup.Messages)
                    {
                        Console.WriteLine(msg);
                    }

                    if (!IsOutputRedirected)
                    {
                        Console.CursorVisible = true;
                    }

                    break;
                }

                if (MasterToken.IsCancellationRequested && counter++ % 10 == 6)
                {
                    Container.StartQueuedSessions();
                }

                if (!IsOutputRedirected)
                {
                    await Task.Delay(100);

                    Console.SetCursorPosition(0, Console.CursorTop - 1);
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

        private static void ShowOverallTime(bool showRemaining, int paddingBufferSize, Stopwatch sw, ref double totalProg, int count, ref double runningRem, int runningCount, bool isCompleted)
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

            if (!isCompleted && totalProg > 0)
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

        private void GetSessionInfo(int bufferWidth, ConsoleFrameBuffer animations, ISession session, out Color color, out string prog, out string desc)
        {
            color = Color.DarkCyan;
            prog = animations.Waiting;

            if (session.IsRunning)
            {
                color = Color.Yellow;
                prog = session.IsPaused ? animations.Paused : session.Progress <= 0 || session.Progress > 100.00 ? animations.Loading : $"{Math.Round(session.Progress, 2)}%".PadBoth(6);
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
                        prog = "failed";
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

                string time;
                if (Verbose && session.Timer != null && (((time = ((double)session.Timer.ElapsedMilliseconds / 1000).ToString("N1")) != "0.0") || session.EndReason == SessionEndReason.Completed))
                {
                    prog += $" in {time}s";
                }
            }

            desc = session.Description;

            if (!IsOutputRedirected)
            {
                int expectedLength = bufferWidth - prog.Length - 5;
                desc = desc.Shrink(expectedLength);
            }
        }

        private async Task ReadKey()
        {
            if (_readerRunning) return;
            _readerRunning = true;

            bool brk = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            while (true)
            {
                KeyQueue = Console.ReadKey(true);

                await Task.Delay(50);

                if (!HasRequest)
                {
                    break;
                }
            }

            _readerRunning = false;
        }

        protected Task ShowProgress(bool showOverall, bool showRemaining)
        {
            Task pTask = ShowProgressImpl(showOverall, showRemaining);

            return pTask;
        }

        protected Task ShowProgressMonitored(bool showOverall, bool showRemaining = true)
        {
            Task pTask = ShowProgress(showOverall, showRemaining);
            MonitorTask(pTask);
            return pTask;
        }

        protected Task ShowProgress(SessionGroup sessionGroup)
        {
            Task pTask = ShowProgressNewImpl(sessionGroup);

            return pTask;
        }

        protected Task ShowProgressMonitored(SessionGroup sessionGroup)
        {
            Task pTask = ShowProgress(sessionGroup);
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

        public override async Task<TResponse> OnSessionRequest<TResponse>(ISession session, SessionRequest<TResponse> request, CancellationToken cancellationToken)
        {
            await Task.Delay(2);

            lock (_requestLock)
            {
                return RequestHandler(session, request, cancellationToken).Result;
            }
        }

        private async Task<TResponse> RequestHandler<TResponse>(ISession session, SessionRequest<TResponse> request, CancellationToken cancellationToken)
        {
            if (!session.IsPaused)
            {
                throw new InvalidOperationException("The session must be paused before send request");
            }

            if (request.DefaultValue is bool)
            {
                Request = new PauseRequest(request.Message, request.DefaultValue.Cast<bool>());

                if (!NoInteractions && !_readerRunning)
                {
                    _ = Task.Run(ReadKey);
                }

                while (!Request.IsResponsed)
                {
                    await Task.Delay(5, cancellationToken);
                }

                return Request.Response.Cast<TResponse>();
            }
            else
            {
                throw new NotSupportedException("The request type is not supported");
            }
        }
    }
}