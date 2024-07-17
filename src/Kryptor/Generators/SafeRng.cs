using System;
using System.Collections.Generic;
using System.Linq;

namespace SAPTeam.Kryptor.Generators
{
    /// <summary>
    /// Generates random bytes array with additional safety.
    /// </summary>
    public class SafeRng : IGenerator
    {
        private static Random random = new Random();

        /// <inheritdoc/>
        public void Generate(byte[] buffer)
        {
            int sampleSize = 256;

            List<byte[]> result = new List<byte[]>();
            int tries = 0;
            int totalTries = 0;

            for (int i = 0; i < buffer.Length; i += sampleSize)
            {
                byte[] sample = new byte[sampleSize];
                random.NextBytes(sample);
                Transformers.Shuffle(sample, buffer.Length + totalTries + i);

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

                totalTries++;
            }

            result.SelectMany((k) => k).Take(buffer.Length).ToArray().CopyTo(buffer, 0);
        }
    }
}
