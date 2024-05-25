using System;
using System.Text;

using Newtonsoft.Json;

namespace SAPTeam.Kryptor
{
    internal static class InternalExtensions
    {
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
    }
}
