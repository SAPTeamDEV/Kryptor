using System;
using System.Collections.Generic;
using System.Text;

namespace Kryptor
{
    internal static class Extensions
    {
        /// <summary>
        /// Divides a string into chunks of a specified size.
        /// </summary>
        /// <param name="str">
        /// The string to divide.
        /// </param>
        /// <param name="maxChunkSize">
        /// The maximum size of each chunk.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of strings, each containing a chunk of the original string.
        /// </returns>
        static public IEnumerable<string> Divide(this string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
    }
}
