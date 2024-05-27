using System;
using System.Drawing;

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

#if NETFRAMEWORK
        internal static string Color(this string src, ConsoleColor color)
        {
            if (color == ConsoleColor.Green)
            {
                return src.Pastel(System.Drawing.Color.GreenYellow);
            }

            return src.Pastel(color);
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
