using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.CommonTK.Console;

namespace SAPTeam.Kryptor.Tool
{
    internal static class Helper
    {
        public static void ShowProgress(int progress)
        {
            ClearLine();
            Echo(new Colorize($"[{progress}%] done", progress < 100 ? ConsoleColor.Yellow : ConsoleColor.Green));
        }

        public static string GetNewFileName(string path, string origName)
        {
            string destination = Path.Combine(Directory.GetParent(path).FullName, origName);
            int suffix = 2;

            while (File.Exists(destination))
            {
                string tempName = $"{Path.GetFileNameWithoutExtension(destination)} ({suffix++}){Path.GetExtension(destination)}";

                if (!File.Exists(Path.Combine(Directory.GetParent(path).FullName, tempName)))
                {
                    destination = Path.Combine(Directory.GetParent(path).FullName, tempName);
                }
            }

            return destination;
        }

        public static string GetVersionString(Assembly assembly)
        {
            var ver = new Version(assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            return string.Join('.', ver.Major, ver.Minor, ver.Build);
        }
    }
}
