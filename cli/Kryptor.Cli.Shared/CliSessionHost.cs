using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;



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
            }
        }

        protected async Task ShowProgress(bool showOverall)
        {
            var sw = Stopwatch.StartNew();

            DebugLog($"Buffer width: {Console.BufferWidth}");
            DebugLog($"Window width: {Console.WindowWidth}");
            DebugLog("");

            var monitor = Task.WhenAll(Container.Tasks);
            var extraLines = 0;

            if (showOverall)
            {
                extraLines++;
            }

            while (true)
            {
                var lines = Container.Sessions.Length + extraLines;

                double totalProg = 0;
                int count = 0;

                double runningProg = 0;
                double runningRem = 0;
                int runningCount = 0;

                foreach (var session in Container.Sessions)
                {
                    if (session.Status == SessionStatus.Running || session.Status == SessionStatus.NotStarted || (session.Status == SessionStatus.Ended && (session.EndReason == SessionEndReason.Completed || session.EndReason == SessionEndReason.Cancelled)))
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
                        prog = Math.Round(session.Progress, 2).ToString() + "%";
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

                    Console.WriteLine($"[{prog.Color(color)}] {session.Description}".PadRight(Console.BufferWidth));
                };

                if (showOverall)
                {
                    totalProg = count > 0 ? Math.Round(totalProg / count, 2) : 0;
                    runningRem = runningCount > 0 ? runningRem / runningCount : 0;
                    var elapsedTime = sw.Elapsed;
                    var remainingTime = TimeSpan.FromMilliseconds(runningRem);

                    Console.WriteLine(((totalProg > 0 ? $"[{totalProg}%] " : "") + $"Elapsed: {elapsedTime.ToString(@"hh\:mm\:ss")} Remaining: {remainingTime.ToString(@"hh\:mm\:ss")}").PadRight(Console.BufferWidth));
                }

                if (monitor.IsCompleted)
                {
                    break;
                }

                await Task.Delay(100);

                Console.CursorLeft = 0;
                Console.CursorTop -= lines;
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