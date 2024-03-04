using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Kryptor
{
    public static class Extensions
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
        /// <param name="srcString">The string to be encrypted</param>
        /// <returns></returns>
        public static string Sha256(this byte[] src)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes_sha256_out = sha256.ComputeHash(src);
                string str_sha256_out = BitConverter.ToString(bytes_sha256_out);
                str_sha256_out = str_sha256_out.Replace("-", "");
                return str_sha256_out;
            }
        }

        /// <summary>
        /// SHA256 encrypt
        /// </summary>
        /// <param name="srcString">The string to be encrypted</param>
        /// <returns></returns>
        public static byte[] RawSha256(this byte[] src)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(src);
            }
        }

        public static byte[] CheckPads(this byte[] src)
        {
            List<byte> buf = new List<byte>();
            List<byte> sus = new List<byte>();

            foreach (var b in src)
            {
                if (b > 0)
                {
                    if (sus.Count > 0)
                    {
                        buf.AddRange(sus);
                        sus.Clear();
                    }

                    buf.Add(b);
                }
                else
                {
                    sus.Add(0);
                }
            }

            return buf.ToArray();
        }

        public static string ToReadable(this int x)
        {
            if (x >= 1073741824)
            {
                return $"{string.Format("{0:0.00}", (double)x / 1073741824)} GiB";
            }
            if (x >= 1048576)
            {
                return $"{string.Format("{0:0.00}", (double)x / 1048576)} MiB";
            }
            if (x >= 1024)
            {
                return $"{string.Format("{0:0.00}", (decimal)x / 1024)} KiB";
            }

            return $"{x} Bytes";
        }

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
