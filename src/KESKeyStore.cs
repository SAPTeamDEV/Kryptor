using System;
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

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">
        /// The index of the key to get.
        /// </param>
        /// <returns>
        /// The key at the specified index.
        /// </returns>
        public string this[int index]
        {
            get
            {
                if (index >= Keys.Length)
                {
                    index -= index / Keys.Length * Keys.Length;
                }

                return Keys[index];
            }
        }

        /// <summary>
        /// The keys to store.
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KESKeyStore"/> struct.
        /// </summary>
        /// <param name="keys">
        /// The keys to store.
        /// </param>
        public KESKeyStore(string[] keys)
        {
            Keys = keys;
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
            string[] keys = new string[count];
            for (int i = 0; i < count; i++)
            {
                keys[i] = Convert.ToBase64String(GetRandomKey(255));
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
            return string.Join(";", Keys);
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
            return new KESKeyStore(s.Trim(new char[] { '\n', '\r' }).Split(';'));
        }
    }
}
