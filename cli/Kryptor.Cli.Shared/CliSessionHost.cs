using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;

using System.Threading;
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

            // AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        protected virtual void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            Log($"{ex.GetType().Name.Color(Color.Red)}: {ex.Message}");

            if (!Verbose)
            {
                Environment.Exit(255);
            }
        }

        public override void Start()
        {
            Log($"Kryptor Command-Line Interface v{Program.Context.CliVersion.Color(Color.Cyan)}");
            Log($"Engine version: {Program.Context.EngineVersion.Color(Color.Cyan)}");
        }

        protected void Log(string message)
        {
            Console.WriteLine(message);
        }

        protected void DebugLog(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
                File.AppendAllText("debug.txt", DateTime.Now.ToString() + " - " + message + "\r\n");
            }
        }

        protected async Task ShowProgress(bool showOverall)
        {
            bool isRedirected = Console.IsOutputRedirected;
            int bufferWidth = Console.BufferWidth;
            int paddingBufferSize = isRedirected ? 1 : bufferWidth;

            Stopwatch sw = null;

            List<string> loadingSteps = new List<string>()
            {
                "-",
                "\\",
                "|",
                "/",
            };
            int loadingStep = 0;

            if (!isRedirected)
            {
                Console.CursorVisible = false;
            }

            if (!isRedirected && bufferWidth < 50)
            {
                showOverall = false;
            }

            var extraLines = 0;

            if (showOverall)
            {
                sw = Stopwatch.StartNew();
                extraLines++;
            }

            List<ISession> sessions = Container.Sessions.ToList();
            List<ISession> flaggedSessions = new List<ISession>();

            var lines = Container.Sessions.Length + extraLines;

            while (true)
            {
                double totalProg = 0;
                int count = 0;

                double runningProg = 0;
                double runningRem = 0;
                int runningCount = 0;

                foreach (var session in sessions)
                {
                    if (session.Progress >= 0
                        && (session.Status == SessionStatus.Running
                            || session.Status == SessionStatus.NotStarted
                            || (session.Status == SessionStatus.Ended && (session.EndReason == SessionEndReason.Completed
                                                                          || session.EndReason == SessionEndReason.Cancelled))))
                    {
                        var sProg = session.Progress;

                        totalProg += sProg;
                        count++;

                        if (session.Status == SessionStatus.Running)
                        {
                            runningProg += sProg;
                            runningRem += Utilities.CalculateRemainingTime(sProg, session.Timer.ElapsedMilliseconds);
                            runningCount++;
                        }
                    }

                    Color color = Color.LightSlateGray;
                    string prog = "waiting";

                    if (session.Status == SessionStatus.Running)
                    {
                        color = Color.Yellow;
                        prog = session.Progress == -1 ? $" {loadingSteps[loadingStep]} " : (Math.Round(session.Progress, 2).ToString() + "%");
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
                                color = Color.OrangeRed;
                                prog = "cancelled";
                                break;
                            case SessionEndReason.Skipped:
                                color = Color.DimGray;
                                prog = "skipped";
                                break;
                            default:
                                color = Color.DarkRed;
                                prog = "unknown";
                                break;
                        }
                    }

                    string desc = session.Description;
                    int expectedLength = bufferWidth - prog.Length - 5;

                    if (!isRedirected && desc.Length > expectedLength)
                    {
                        desc = "..." + desc.Substring(desc.Length - expectedLength + 3);
                    }

                    if (!isRedirected || session.Status == SessionStatus.Ended)
                    {
                        Console.WriteLine($"[{prog.Color(color)}] {desc}".PadRight(paddingBufferSize));

                        if (isRedirected)
                        {
                            flaggedSessions.Add(session);
                        }
                    }
                };

                foreach (var session in flaggedSessions)
                {
                    sessions.Remove(session);
                }
                flaggedSessions.Clear();

                bool isCompleted = sessions.All(x => x.Status == SessionStatus.Ended);

                if (showOverall && (!isRedirected || isCompleted))
                {
                    totalProg = count > 0 ? Math.Round(totalProg / count, 2) : 0;
                    runningRem = runningCount > 0 ? runningRem / runningCount : 0;
                    var elapsedTime = sw.Elapsed;
                    var remainingTime = TimeSpan.FromMilliseconds(runningRem);

                    Console.WriteLine(((totalProg > 0 ? $"[{totalProg}%] " : "") + $"Elapsed: {elapsedTime.ToString(@"hh\:mm\:ss")} Remaining: {remainingTime.ToString(@"hh\:mm\:ss")}").PadRight(paddingBufferSize));
                }

                loadingStep = (loadingStep + 1) % loadingSteps.Count;

                if (isCompleted)
                {
                    sw?.Stop();

                    if (!isRedirected)
                    {
                        Console.CursorVisible = true;
                    }

                    break;
                }

                if (!isRedirected)
                {
                    await Task.Delay(100);

                    Console.CursorLeft = 0;
                    Console.CursorTop -= lines;
                }
            }
        }

        protected Task ShowProgressMonitored(bool showOverall)
        {
            var pTask = ShowProgress(showOverall);
            MonitorTask(pTask);
            return pTask;
        }
    }
}