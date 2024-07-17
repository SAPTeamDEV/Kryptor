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
        static byte[] entropy = new byte[8192];
        static CryptoRandom crng = new CryptoRandom();
        static readonly SHA512 _sha512;

        static EntroX()
        {
            // Initialize entropy with some random data.
            crng.NextBytes(entropy);
        }

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
                Array.Copy(data, 0, buffer, pos, Math.Min(data.Length, buffer.Length - pos);
                pos += data.Length;

                tries++;
            }

            UpdateEntropy();

        }

        static void TransformEntropy()
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
            }

            UpdateEntropy();

            byte[] tData = _sha512.ComputeHash(picked);
            return tData;
        }

        public static void AddEntropy(byte[] data)
        {
            AddEntropyInternal(data);
            UpdateEntropy();
        }

        static void AddEntropyInternal(byte[] data)
        {
            foreach (var i in data)
            {
                int n = crng.Next(entropy.Length);
                entropy[n] = i;
            }
        }

        static void UpdateEntropy()
        {
            int i = 0;
            while (i <= crng.Next(6, 12))
            {
                AddEntropyInternal(_sha512.ComputeHash(entropy));
            }
        }
    }
}
