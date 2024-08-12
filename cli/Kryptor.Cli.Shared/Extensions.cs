using System;
using System.Drawing;
using System.Runtime.InteropServices;


#if NETFRAMEWORK
using Pastel;
#endif

namespace SAPTeam.Kryptor.Cli
{
    internal static class Extensions
    {
        internal static string FormatFingerprint(this byte[] src)
        {
            return BitConverter.ToString(src).Replace("-", ":");
        }

        internal static string ToLowerIfUnix(this string src)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? src : src.ToLower();
        }

#if NETFRAMEWORK
        internal static string Color(this string src, ConsoleColor color)
        {
            return color == ConsoleColor.Green ? src.Pastel(System.Drawing.Color.GreenYellow) : src.Pastel(color);
        }

        internal static string Color(this string src, Color color)
        {
            if (color == System.Drawing.Color.LightGoldenrodYellow)
            {
                color = System.Drawing.Color.Cyan;
            }

            return src.Pastel(color);
        }
#endif
    }
}
