using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Divides a string into chunks of a specified size.
        /// </summary>
        /// <param name="source">
        /// The string to divide.
        /// </param>
        /// <param name="maxChunkSize">
        /// The maximum size of each chunk.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of strings, each containing a chunk of the original string.
        /// </returns>
        static public IEnumerable<T[]> Slice<T>(this Array source, int maxChunkSize)
        {
            for (int i = 0; i < source.Length; i += maxChunkSize)
            {
                var actualSize = Math.Min(source.Length - i, maxChunkSize);
                T[] slice = new T[actualSize];
                Array.Copy(source, i, slice, 0, actualSize);
                yield return slice;
            }
        }

        /// <summary>
        /// SHA256 encrypt
        /// </summary>
        /// <param name="src">The string to be encrypted</param>
        /// <returns></returns>
        public static byte[] RawSha256(this byte[] src)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(src);
            }
        }

        public static int LocatePattern(this byte[] src, byte[] pattern)
        {
            for (int i = 0; i < src.Length; i++)
            {
                if (src.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
