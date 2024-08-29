using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

using Pastel;

using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Cli
{
    internal static partial class Extensions
    {
        static CryptoRandom _crng = new CryptoRandom();

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

        // Copied from MoreLinq

        /// <summary>
        /// Immediately executes the given action on each element in the source sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence</typeparam>
        /// <param name="source">The sequence of elements</param>
        /// <param name="action">The action to execute on each element</param>

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var element in source)
                action(element);
        }

        /// <summary>
        /// Returns a sequence of elements in random order from the original
        /// sequence.
        /// </summary>
        /// <typeparam name="T">The type of source sequence elements.</typeparam>
        /// <param name="source">
        /// The sequence from which to return random elements.</param>
        /// <returns>
        /// A sequence of elements <paramref name="source"/> randomized in
        /// their order.
        /// </returns>
        /// <remarks>
        /// This method uses deferred execution and streams its results. The
        /// source sequence is entirely buffered before the results are
        /// streamed.
        /// </remarks>

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return Shuffle(source, _crng);
        }

        /// <summary>
        /// Returns a sequence of elements in random order from the original
        /// sequence. An additional parameter specifies a random generator to be
        /// used for the random selection algorithm.
        /// </summary>
        /// <typeparam name="T">The type of source sequence elements.</typeparam>
        /// <param name="source">
        /// The sequence from which to return random elements.</param>
        /// <param name="rand">
        /// A random generator used as part of the selection algorithm.</param>
        /// <returns>
        /// A sequence of elements <paramref name="source"/> randomized in
        /// their order.
        /// </returns>
        /// <remarks>
        /// This method uses deferred execution and streams its results. The
        /// source sequence is entirely buffered before the results are
        /// streamed.
        /// </remarks>

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rand)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rand == null) throw new ArgumentNullException(nameof(rand));

            return RandomSubsetImpl(source, rand, subsetSize: null);
        }
    }
}
