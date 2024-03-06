using System;
using System.Text;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Stores keys for the <see cref="KESProvider"/> class.
    /// </summary>
    public struct KeyStore
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
        /// Initializes a new instance of the <see cref="KeyStore"/> struct.
        /// </summary>
        /// <param name="keys">
        /// The keys to store.
        /// </param>
        public KeyStore(string[] keys)
        {
            Keys = keys;
        }

        /// <summary>
        /// Generates a new <see cref="KeyStore"/> instance.
        /// </summary>
        /// <param name="count">
        /// The number of keys to generate.
        /// </param>
        /// <param name="keySize">
        /// The size of the keys to generate.
        /// </param>
        /// <returns>
        /// The new <see cref="KeyStore"/> instance.
        /// </returns>
        public static KeyStore Generate(int count = 128, int keySize = 256)
        {
            string[] keys = new string[count];
            for (int i = 0; i < count; i++)
            {
                keys[i] = GetRandomStr(keySize / 8);
            }

            return new KeyStore(keys);
        }

        private static string GetRandomStr(int length)
        {
            char[] arrChar = new char[]{
           'a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x',
           '0','1','2','3','4','5','6','7','8','9',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z'
          };

            StringBuilder num = new StringBuilder();

            if (random == null)
            {
                random = new Random();
            }

            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[random.Next(0, arrChar.Length)].ToString());
            }

            return num.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join(";", Keys);
        }

        /// <summary>
        /// Converts a string to a <see cref="KeyStore"/> instance.
        /// </summary>
        /// <param name="s">
        /// The string to convert.
        /// </param>
        /// <returns>
        /// The <see cref="KeyStore"/> instance.
        /// </returns>
        public static KeyStore FromString(string s)
        {
            return new KeyStore(s.Trim(new char[] { '\n', '\r' }).Split(';'));
        }
    }
}
