using System;
using System.Collections.Generic;
using System.Linq;

using EnsureThat;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Stores keys for the <see cref="Kes"/> class.
    /// </summary>
    public struct KeyStore
    {
        private static readonly Random random = new Random();
        private readonly int count;

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">
        /// The index of the key to get.
        /// </param>
        /// <returns>
        /// The key at the specified index.
        /// </returns>
        public byte[] this[int index] => Keys.ElementAt(Math.Abs(index) % count);

        /// <summary>
        /// Gets the keys to store.
        /// </summary>
        public IEnumerable<byte[]> Keys { get; }

        /// <summary>
        /// Gets the raw flat bytes array of data.
        /// </summary>
        public byte[] Raw { get; }

        /// <summary>
        /// Gets the unique fingerprint of this keystore.
        /// </summary>
        public byte[] Fingerprint { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyStore"/> struct.
        /// </summary>
        /// <param name="bytes">
        /// The keys to store.
        /// </param>
        public KeyStore(byte[] bytes)
        {
            Ensure.Enumerable.HasItems(bytes, nameof(bytes));

            Raw = bytes;
            Keys = Raw.Chunk(32).Where(x => x.Length == 32);
            count = Keys.Count();

            Fingerprint = Raw.Sha256().Skip(8).Take(16).ToArray();
        }

        /// <summary>
        /// Initializes a new <see cref="KeyStore"/> instance with random key.
        /// </summary>
        /// <param name="count">
        /// The number of keys to generate.
        /// </param>
        /// <returns>
        /// The new <see cref="KeyStore"/> instance.
        /// </returns>
        public static KeyStore Generate(int count = 0)
        {
            if (count <= 0)
            {
                count = GetRandomOddNumber();
            }

            var result = SafeRng.Generate(count * 32);

            return new KeyStore(result);
        }

        /// <summary>
        /// Returns a random odd number between 257 and 2047.
        /// </summary>
        /// <returns></returns>
        public static int GetRandomOddNumber()
        {
            int count = random.Next(257, 2047);
            if (count % 2 == 0)
            {
                count++;
            }

            return count;
        }
    }
}
