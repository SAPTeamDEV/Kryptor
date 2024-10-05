namespace SAPTeam.Kryptor.InteractiveTests
{
    public static class Utilities
    {
        public static IEnumerable<string> Divide(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
            {
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
            }
        }

        public static string ToReadable(this int x)
        {
            return x >= 1073741824
                ? $"{string.Format("{0:0.00}", (double)x / 1073741824)} GiB"
                : x >= 1048576
                ? $"{string.Format("{0:0.00}", (double)x / 1048576)} MiB"
                : x >= 1024 ? $"{string.Format("{0:0.00}", (decimal)x / 1024)} KiB" : $"{x} Bytes";
        }
    }
}
