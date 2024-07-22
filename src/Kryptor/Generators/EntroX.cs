using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Generators
{
    /// <summary>
    /// Generates random data reinforced by the entropies.
    /// </summary>
    public class EntroX : IGenerator
    {
        private static readonly byte[] entropy = new byte[8192];
        private static readonly CryptoRandom crng = new CryptoRandom();
        private static readonly SHA512 _sha512;

        static EntroX()
        {
            // Initialize entropy with some random data.
            crng.NextBytes(entropy);

            _sha512 = SHA512.Create();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntroX"/> class.
        /// </summary>
        /// <param name="entropies">
        /// The entropy data to randomly spread in the global entropy to improve randomness.
        /// </param>
        public EntroX(params byte[][] entropies)
        {
            foreach (byte[] data in entropies)
            {
                AddEntropyInternal(data);
            }

            UpdateEntropy();
        }

        /// <inheritdoc/>
        public void Generate(byte[] buffer)
        {
            TransformEntropy();

            int tries = 1;
            int pos = 0;
            while (pos < buffer.Length)
            {
                if (tries % 10 == 0)
                {
                    TransformEntropy();
                }
                var data = _sha512.ComputeHash(entropy).Concat(QueryEntropy(24, 96)).ToArray();
                Array.Copy(data, 0, buffer, pos, Math.Min(data.Length, buffer.Length - pos));
                pos += data.Length;

                tries++;
            }

            UpdateEntropy();

        }

        private static void TransformEntropy()
        {
            int i = 0;
            while (i < entropy.Length)
            {
                byte[] tData = QueryEntropy(86, 384);
                tData.CopyTo(entropy, i);
                i += tData.Length;
            }

            UpdateEntropy();
        }

        private static byte[] QueryEntropy(int min, int max)
        {
            byte[] picked = new byte[crng.Next(min, max)];
            int j = 0;

            while (j < picked.Length)
            {
                picked[j] = entropy[crng.Next(entropy.Length)];
                j++;
            }

            UpdateEntropy();

            byte[] tData = _sha512.ComputeHash(picked);
            return tData;
        }

        /// <summary>
        /// Adds new entropies to the global entropy in random order.
        /// </summary>
        /// <param name="data">
        /// The entropy data to randomly spread in the global entropy to improve randomness.
        /// </param>
        public static void AddEntropy(byte[] data)
        {
            AddEntropyInternal(data);
            UpdateEntropy();
        }

        private static void AddEntropyInternal(byte[] data)
        {
            foreach (var i in data)
            {
                int n = crng.Next(entropy.Length);
                entropy[n] = i;
            }
        }

        private static void UpdateEntropy()
        {
            int i = 0;
            int max = crng.Next(6, 12);
            while (i <= max)
            {
                AddEntropyInternal(_sha512.ComputeHash(entropy));
                i++;
            }
        }
    }
}
