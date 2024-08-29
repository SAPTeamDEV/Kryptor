using System.Text;

namespace SAPTeam.Kryptor.Extensions
{
    internal static class InternalExtensions
    {
        internal static byte[] Encode(this string src) => Encoding.UTF8.GetBytes(src);

        internal static string Decode(this byte[] src) => Encoding.UTF8.GetString(src);

        internal static string Base64Encode(this byte[] src) => Convert.ToBase64String(src);

        internal static string Base64Encode(this string src) => src.Encode().Base64Encode();

        internal static byte[] Base64EncodeToByte(this string src) => src.Base64Encode().Encode();

        internal static byte[] Base64EncodeToByte(this byte[] src) => src.Base64Encode().Encode();

        internal static byte[] Base64Decode(this string src) => Convert.FromBase64String(src);

        internal static byte[] Base64Decode(this byte[] src) => src.Decode().Base64Decode();

        internal static string Base64DecodeToString(this byte[] src) => src.Base64Decode().Decode();

        internal static string Base64DecodeToString(this string src) => src.Base64Decode().Decode();
    }
}
