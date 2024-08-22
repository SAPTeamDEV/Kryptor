using System;

using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents a unified header for kryptor front ends.
    /// </summary>
    public class ClientHeader : Header
    {
        private static CryptoRandom crng = new CryptoRandom();

        /// <summary>
        /// Gets or sets the name of the encryptor client application.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the version of the encryptor client application.
        /// </summary>
        public Version ClientVersion { get; set; }

        /// <summary>
        /// Gets or sets the original file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the encrypted file.
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Generates a new unique identifier for the header and associates it with the <see cref="Serial"/> property.
        /// </summary>
        public void GenerateSerial()
        {
            string[] serial = new string[8];
            for (int i = 0; i < serial.Length; i++)
            {
                serial[i] = crng.Next(0x1869F).ToString("D5");
            }

            Serial = string.Join("-", serial);
        }
    }
}
