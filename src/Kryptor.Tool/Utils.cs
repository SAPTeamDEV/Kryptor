using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.CommonTK.Console;

namespace SAPTeam.Kryptor.Tool
{
    internal static class Helper
    {
        public static void ShowProgress(int progress)
        {
            ClearLine(true);
            Echo(new Colorize($"[{progress}%] done", progress < 100 ? ConsoleColor.Yellow : ConsoleColor.Green), progress == 100);
        }
    }
}
