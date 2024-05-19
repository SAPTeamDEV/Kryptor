using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Generates random bytes array with additional safety.
    /// </summary>
    public static class SafeRng
    {
        private static Random random = new Random();

        /// <summary>
        /// Generates random bytes array.
        /// </summary>
        /// <param name="length">
        /// Length of the output array.
        /// </param>
        /// <returns></returns>
        public static byte[] Generate(int length)
        {
            int sampleSize = 32;

            List<byte[]> result = new List<byte[]>();
            int tries = 0;

            for (int i = 0; i < length; i += sampleSize)
            {
                byte[] sample = new byte[sampleSize];
                random.NextBytes(sample);

                // Ignore keys with 10 or more duplicated items.
                if (result.All((b) => b.Intersect(sample).Count() < 10) || tries > 100)
                {
                    result.Add(sample);
                    tries = 0;
                }
                else
                {
                    tries++;
                    i -= sampleSize;
                }
            }

            return result.SelectMany((k) => k).Take(length).ToArray();
        }
    }
}
