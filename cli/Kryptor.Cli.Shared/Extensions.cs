using System;
using System.Drawing;
using System.Runtime.InteropServices;

#if NETFRAMEWORK
using Pastel;
#else
using ANSIConsole;
#endif

namespace SAPTeam.Kryptor.Cli
{
    internal static class Extensions
    {
        internal static string FormatFingerprint(this byte[] src) => BitConverter.ToString(src).Replace("-", ":");

        public static string PadBoth(this string str, int length)
        {
            int spaces = length - str.Length;
            int padLeft = spaces / 2 + str.Length;
            return str.PadLeft(padLeft).PadRight(length);
        }

        internal static string ToLowerIfUnix(this string src) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? src : src.ToLower();

#if NETFRAMEWORK
        static string ColorImpl(this string src, ConsoleColor color) => color == ConsoleColor.Green ? src.Pastel(System.Drawing.Color.GreenYellow) : src.Pastel(color);

        static string ColorImpl(this string src, Color color) => src.Pastel(color);
#else
        private static string ColorImpl(this string src, ConsoleColor color) => src.Color(color).ToString();

        private static string ColorImpl(this string src, Color color) => src.Color(color).ToString();
#endif

        public static string WithColor(this string src, ConsoleColor color)
        {
            if (Program.Context.NoColor)
            {
                return src;
            }

            return src.ColorImpl(color);
        }

        public static string WithColor(this string src, Color color)
        {
            if (Program.Context.NoColor)
            {
                return src;
            }

            return src.ColorImpl(color);
        }
    }
}
