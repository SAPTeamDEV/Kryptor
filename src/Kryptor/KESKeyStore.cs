using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Stores keys for the <see cref="KESProvider"/> class.
    /// </summary>
    public struct KESKeyStore
    {
        private static Random random;
        readonly int keystoreLength;

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">
        /// The index of the key to get.
        /// </param>
        /// <returns>
        /// The key at the specified index.
        /// </returns>
        public byte[] this[int index]
        {
            get
            {
                if (index >= keystoreLength)
                {
                    index -= (index / keystoreLength) * keystoreLength;
                }

                return Keys.ElementAt(index);
            }
        }

        /// <summary>
        /// The keys to store.
        /// </summary>
        public IEnumerable<byte[]> Keys { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KESKeyStore"/> struct.
        /// </summary>
        /// <param name="keys">
        /// The keys to store.
        /// </param>
        public KESKeyStore(IEnumerable<byte[]> keys)
        {
            Keys = keys;
            keystoreLength = Keys.Count();
        }

        /// <summary>
        /// Generates a new <see cref="KESKeyStore"/> instance.
        /// </summary>
        /// <param name="count">
        /// The number of keys to generate.
        /// </param>
        /// <returns>
        /// The new <see cref="KESKeyStore"/> instance.
        /// </returns>
        public static KESKeyStore Generate(int count = 128)
        {
            byte[][] keys = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                keys[i] = GetRandomKey(32);
            }

            return new KESKeyStore(keys);
        }

        private static byte[] GetRandomKey(int length)
        {
            byte[] buffer = new byte[length];

            if (random == null)
            {
                random = new Random();
            }

            random.NextBytes(buffer);

            return buffer;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join(";", Keys.Select(x => Convert.ToBase64String(x)));
        }

        /// <summary>
        /// Converts a string to a <see cref="KESKeyStore"/> instance.
        /// </summary>
        /// <param name="s">
        /// The string to convert.
        /// </param>
        /// <returns>
        /// The <see cref="KESKeyStore"/> instance.
        /// </returns>
        public static KESKeyStore FromString(string s)
        {
            return new KESKeyStore(s.Trim(new char[] { '\n', '\r', '\0' }).Split(';').Select(x => Convert.FromBase64String(x)));
        }
    }
}
