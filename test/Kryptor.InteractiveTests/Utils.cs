namespace ConsoleApp1
{
    public static class Utils
    {
        static public IEnumerable<string> Divide(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
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
    }
}
