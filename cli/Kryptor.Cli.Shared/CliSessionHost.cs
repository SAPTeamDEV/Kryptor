using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;

using System.Threading;


#if !NETFRAMEWORK
using ANSIConsole;
#endif

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliSessionHost : SessionHost
    {
        public CliSessionHost()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        protected virtual void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            
        }

        public override void Start()
        {
            Console.WriteLine($"Kryptor Command-Line Interface v{Program.Context.CliVersion.Color(Color.Cyan)}");
            Console.WriteLine($"Engine version: {Program.Context.EngineVersion.Color(Color.Cyan)}");
        }

        protected async Task ShowProgress()
        {
            var monitor = Task.WhenAll(Container.Tasks);
            var lines = Container.Sessions.Length;

            while (true)
            {
                foreach (var session in Container.Sessions)
                {
                    Color color = Color.LightSlateGray;
                    string prog = "waiting";

                    if (session.Status == SessionStatus.Running)
                    {
                        color = Color.Yellow;
                        prog = Math.Round(session.Progress, 2).ToString();
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

                if (monitor.IsCompleted)
                {
                    break;
                }

                await Task.Delay(100);

                Console.CursorTop -= lines;
            }
        }
    }
}