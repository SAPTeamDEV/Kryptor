using System;

#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
#endif

using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents a unified header for kryptor front ends.
    /// </summary>
    public class ClientHeader : Header
    {
        private static readonly CryptoRandom crng = new CryptoRandom();

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        protected override JsonSerializerContext JsonSerializerContext => SourceGenerationClientHeaderContext.Default;
#endif

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
            string[] serial = new string[10];
            for (int i = 0; i < serial.Length; i++)
            {
                serial[i] = crng.Next(0x1869F).ToString("X5");
            }

            Serial = string.Join("-", serial);
        }
    }

#if NET6_0_OR_GREATER
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Header))]
    [JsonSerializable(typeof(ClientHeader))]
    internal partial class SourceGenerationClientHeaderContext : JsonSerializerContext
    {
    }
#endif
}
