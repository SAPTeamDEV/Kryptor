using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// SHA256 encrypt
        /// </summary>
        /// <param name="src">The string to be encrypted</param>
        /// <returns></returns>
        public static byte[] Sha256(this byte[] src)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(src);
            }
        }

        /// <summary>
        /// Finds given pattern in a byte array.
        /// </summary>
        /// <param name="src">
        /// Source byte array.
        /// </param>
        /// <param name="pattern">
        /// The pattern to be searched.
        /// </param>
        /// <returns>Start index of first occurrence.</returns>
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

        /// <summary>
        /// Converts bytes to hex with Fingerprint format.
        /// </summary>
        /// <param name="src">
        /// The source data.
        /// </param>
        public static string FormatFingerprint(this byte[] src)
        {
            return BitConverter.ToString(src).Replace("-", ":");
        }

        internal static byte[] Encode(this string src)
        {
            return Encoding.UTF8.GetBytes(src);
        }

        internal static string Decode(this byte[] src)
        {
            return Encoding.UTF8.GetString(src);
        }

        internal static string Base64Encode(this byte[] src)
        {
            return Convert.ToBase64String(src);
        }

        internal static string Base64Encode(this string src)
        {
            return src.Encode().Base64Encode();
        }

        internal static byte[] Base64EncodeToByte(this string src)
        {
            return src.Base64Encode().Encode();
        }

        internal static byte[] Base64EncodeToByte(this byte[] src)
        {
            return src.Base64Encode().Encode();
        }

        internal static byte[] Base64Decode(this string src)
        {
            return Convert.FromBase64String(src);
        }

        internal static byte[] Base64Decode(this byte[] src)
        {
            return src.Decode().Base64Decode();
        }

        internal static string Base64DecodeToString(this byte[] src)
        {
            return src.Base64Decode().Decode();
        }

        internal static string Base64DecodeToString(this string src)
        {
            return src.Base64Decode().Decode();
        }

        internal static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        internal static T ReadJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
