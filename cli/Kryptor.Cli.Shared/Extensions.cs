using System;
using System.Drawing;
using System.Runtime.InteropServices;

using Pastel;

namespace SAPTeam.Kryptor.Cli
{
    internal static class Extensions
    {
        internal static string FormatFingerprint(this byte[] src) => BitConverter.ToString(src).Replace("-", ":");

        public static string PadBoth(this string str, int length)
        {
            int spaces = length - str.Length;
            int padLeft = (spaces / 2) + str.Length;
            return str.PadLeft(padLeft).PadRight(length);
        }

        internal static string ToLowerIfUnix(this string src) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? src : src.ToLower();

        private static string ColorImpl(this string src, ConsoleColor color) => color == ConsoleColor.Green ? src.Pastel(System.Drawing.Color.GreenYellow) : src.Pastel(color);

        private static string ColorImpl(this string src, Color color) => src.Pastel(color);

        public static string WithColor(this string src, ConsoleColor color) => Program.Context.NoColor ? src : src.ColorImpl(color);

        public static string WithColor(this string src, Color color) => Program.Context.NoColor ? src : src.ColorImpl(color);

        public static string FormatWithCommas(this long number) => number.ToString("N0");

        public static string FormatWithCommas(this int number) => number.ToString("N0");

        public static string FormatWithCommas(this double number) => number.ToString("N2");

        public static string Shrink(this string src, int expectedLength)
        {
            if (src.Length > expectedLength)
            {
                src = $"...{src.Substring(src.Length - expectedLength + 3)}";
            }

            return src;
        }
    }
}
