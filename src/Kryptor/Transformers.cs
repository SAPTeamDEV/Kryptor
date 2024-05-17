using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Contains methods to transform vlues.
    /// </summary>
    public static class Transformers
    {
        /// <summary>
        /// Initializes a new transformer with given token.
        /// </summary>
        /// <param name="token">
        /// The tranformer token.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public static ITranformer GetTranformer(TransformerToken token)
        {
            switch (token.TransformerName)
            {
                case "generica":
                case "gen":
                case "g":
                    return new Generica(token.SecretKey, token.Salt != null ? token.Salt : token.SecretKey);
                default:
                    throw new ArgumentException("Invalid transformer name: " +  token.TransformerName);
            }
        }

        static IEnumerable<T> Rotate<T>(IEnumerable<T> collection, int positions)
        {
            int count = collection.Count();
            if (count == 0)
                yield break;

            // Normalize the rotation amount (handle negative values and large rotations)
            positions = (positions % count + count) % count;

            // Create a rotated view of the collection
            IEnumerator<T> enumerator = collection.GetEnumerator();
            for (int i = 0; i < positions; i++)
            {
                if (!enumerator.MoveNext())
                    enumerator.Reset();
            }

            do
            {
                yield return enumerator.Current;
            } while (enumerator.MoveNext());
        }

        static IEnumerable<T> Pick<T>(IEnumerable<T> collection, int count, int seed)
        {
            Random random = new Random(seed);

            // Perform Fisher-Yates shuffle
            T[] array = collection.ToArray();
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            // Take the first 'count' items from the shuffled array
            return array.Take(Math.Min(count, n));
        }

        static int ToInt32(IEnumerable<byte> collection, int seed)
        {
            return BitConverter.ToInt32(Rotate(collection, seed).ToArray(), 0);
        }

        static long ToInt64(IEnumerable<byte> collection, int seed)
        {
            return BitConverter.ToInt64(Rotate(collection, seed).ToArray(), 0);
        }

        static IEnumerable<T> Mix<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, int seed)
        {
            // Combine the elements from both collections
            List<T> combinedList = collection1.Concat(collection2).ToList();

            // Create a hash function (SHA-256 in this example)
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the seed to bytes
                byte[] seedBytes = BitConverter.GetBytes(seed);

                // Compute the hash of the seed
                byte[] hashBytes = sha256.ComputeHash(seedBytes);

                // Use the hash to shuffle the combined list
                int n = combinedList.Count;
                for (int i = n - 1; i > 0; i--)
                {
                    int j = Math.Abs(BitConverter.ToInt32(hashBytes, i % hashBytes.Length)) % (i + 1);
                    T temp = combinedList[i];
                    combinedList[i] = combinedList[j];
                    combinedList[j] = temp;
                }
            }

            return combinedList;
        }
    }
}
