using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SAPTeam.Kryptor.Extensions;
using SAPTeam.Kryptor.Generators;

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
                    return new Generica(token.SecretKey, token.Salt ?? GenerateSalt(token));
                case "liteen":
                case "lite":
                case "l":
                    return new LiteEn(token.SecretKey, token.Salt ?? GenerateSalt(token));
                default:
                    throw new ArgumentException("Invalid transformer name: " + token.TransformerName);
            }
        }

        private static byte[] GenerateSalt(TransformerToken token)
        {
            return Pick(token.SecretKey.Sha256(), 16, token.Rotate > 0 ? token.KeySize * token.Rotate : token.KeySize).ToArray();
        }

        /// <summary>
        /// Rotates the elements of an array by a specified number of positions.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to rotate.</param>
        /// <param name="positions">The number of positions to rotate (positive or negative).</param>
        public static T[] Rotate<T>(T[] array, int positions)
        {
            int length = array.Length;
            if (length == 0)
            {
                return Array.Empty<T>(); // Nothing to rotate
            }

            // Normalize the rotation amount (handle negative values and large rotations)
            positions = ((positions % length) + length) % length;

            // Create a temporary array to store rotated elements
            T[] rotatedArray = new T[length];

            // Copy elements after rotation to the temporary array
            for (int i = 0; i < length; i++)
            {
                int newIndex = (i + positions) % length;
                rotatedArray[newIndex] = array[i];
            }

            return rotatedArray;
        }

        /// <summary>
        /// Shuffles the elements of a collection using a hash-based random index and then selects secified number of members.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to shuffle.</param>
        /// <param name="count">The number of items to pick from the shuffled collection.</param>
        /// <param name="seed">The seed value for the randomization.</param>
        /// <returns>An enumerable containing the shuffled elements.</returns>
        public static IEnumerable<T> Pick<T>(IEnumerable<T> collection, int count, int seed)
        {
            T[] array = collection.ToArray();
            Shuffle(array, seed);

            // Take the first 'count' items from the shuffled array
            return array.Take(Math.Min(count, array.Length));
        }

        /// <summary>
        /// Combines and shuffles multiple collections into a single array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collections.</typeparam>
        /// <param name="seed">The seed value for the randomization.</param>
        /// <param name="collections">The collections to combine and shuffle.</param>
        /// <returns>An array containing the shuffled elements from all collections.</returns>
        public static T[] Mix<T>(int seed, params IEnumerable<T>[] collections)
        {
            List<T> combinedList = new List<T>();

            foreach (var collection in collections)
            {
                combinedList.AddRange(collection);
            }

            T[] combinedArray = combinedList.ToArray();

            Shuffle(combinedArray, seed);

            return combinedArray;
        }

        /// <summary>
        /// Shuffles an array in-place using a hash-based random index.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to shuffle.</param>
        /// <param name="seed">The seed value for the randomization.</param>
        public static void Shuffle<T>(T[] array, int seed)
        {
            using (var sha256 = SHA256.Create())
            {
                int n = array.Length;

                for (int i = n - 1; i > 0; i--)
                {
                    // Compute a hash-based random index
                    byte[] hashBytes = sha256.ComputeHash(BitConverter.GetBytes(seed + i));
                    int j = Math.Abs(BitConverter.ToInt32(hashBytes, 0)) % (i + 1);

                    // Swap elements
                    (array[j], array[i]) = (array[i], array[j]);
                }
            }
        }

        /// <summary>
        /// Converts a sequence of bytes to a positive 32-bit integer using a hash-based random index.
        /// </summary>
        /// <param name="collection">The collection of bytes.</param>
        /// <param name="seed">The seed value for the randomization.</param>
        /// <returns>The resulting 32-bit integer.</returns>
        public static int ToAbsInt32(IEnumerable<byte> collection, int seed)
        {
            return Math.Abs(ToInt32(collection, seed));
        }

        /// <summary>
        /// Converts a sequence of bytes to a 32-bit integer using a hash-based random index.
        /// </summary>
        /// <param name="collection">The collection of bytes.</param>
        /// <param name="seed">The seed value for the randomization.</param>
        /// <returns>The resulting 32-bit integer.</returns>
        public static int ToInt32(IEnumerable<byte> collection, int seed)
        {
            return BitConverter.ToInt32(Pick(collection, 4, seed).ToArray(), 0);
        }

        /// <summary>
        /// Converts a sequence of bytes to a 64-bit integer using a hash-based random index.
        /// </summary>
        /// <param name="collection">The collection of bytes.</param>
        /// <param name="seed">The seed value for the randomization.</param>
        /// <returns>The resulting 64-bit integer.</returns>
        public static long ToInt64(IEnumerable<byte> collection, int seed)
        {
            return BitConverter.ToInt64(Pick(collection, 8, seed).ToArray(), 0);
        }

        /// <summary>
        /// Creates a transformed key.
        /// </summary>
        /// <param name="keyStore">
        /// The key store to process.
        /// </param>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <returns></returns>
        public static byte[] CreateKey(KeyStore keyStore, CryptoProcess process)
        {
            int seed = ToAbsInt32(process.BlockHash, process.BlockIndex + process.ChunkIndex);
            var sets = Pick(keyStore.Keys, (seed % 8) + 1, seed).SelectMany(x => x);

            var mixed = Mix(seed, sets);
            var key = Pick(mixed, 32, seed);

            return key.ToArray();
        }

        /// <summary>
        /// Creates a transformed IV.
        /// </summary>
        /// <param name="process">
        /// The crypto process data holder.
        /// </param>
        /// <param name="keyStore">
        /// The key store to process.
        /// </param>
        /// <returns></returns>
        public static byte[] CreateIV(KeyStore keyStore, CryptoProcess process)
        {
            return Pick(keyStore.Keys, 1, (process.BlockHash[5] % (process.BlockHash[19] + 4)) - process.ChunkIndex).First().Take(16).ToArray();
        }
    }
}
