using System;
using System.Collections.Generic;
using System.IO;
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
            Echo(new Colorize($"[{progress}%] done", progress < 100 ? ConsoleColor.Yellow : ConsoleColor.Green), false);
        }

        public static string GetNewFileName(string path, string origName)
        {
            string destination = Path.Combine(Directory.GetParent(path).FullName, origName);
            int suffix = 2;

            while (File.Exists(destination))
            {
                string tempName = $"{Path.GetFileNameWithoutExtension(destination)} ({suffix++}).{Path.GetExtension(destination)}";
                if (tempName.EndsWith('.'))
                {
                    tempName = tempName.Substring(0, tempName.Length - 1);
                }
                if (!File.Exists(Path.Combine(Directory.GetParent(path).FullName, tempName)))
                {
                    destination = Path.Combine(Directory.GetParent(path).FullName, tempName);
                }
            }

            return destination;
        }
    }
}
