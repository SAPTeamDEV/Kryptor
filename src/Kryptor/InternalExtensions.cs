using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
