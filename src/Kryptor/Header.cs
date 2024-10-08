using System.Text.Json;
using System.Text.Json.Serialization;

using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the data header.
    /// </summary>
    public class Header
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// The source generated code of this header.
        /// </summary>
        /// <remarks>
        /// IF YOU WANT TO CREATE YOUR OWN HEADER, YOU MUST REPLACE THIS PROPERTY.
        /// </remarks>
        protected virtual JsonSerializerContext JsonSerializerContext => SourceGenerationHeaderContext.Default;
#endif

        /// <summary>
        /// Gets or sets the version of the encryptor api backend.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the version of the encryptor engine.
        /// </summary>
        public Version EngineVersion { get; set; }

        /// <summary>
        /// Gets or sets the amount of details included in this header.
        /// </summary>
        public HeaderVerbosity Verbosity { get; set; }

        /// <summary>
        /// Gets or sets the file block size.
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        /// Gets or sets the crypto provider configuration.
        /// </summary>
        public CryptoProviderConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the extra header entries.
        /// </summary>
        public Dictionary<string, string> Extra { get; set; }

        /// <summary>
        /// Exports the header contents as base64 encoded bytes array with header size.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OverflowException"></exception>
        public byte[] Export()
        {
            string js = ToJson(this);
            byte[] b64 = js.Base64EncodeToByte();

            if (b64.Length > 65535)
            {
                throw new OverflowException("Huge header size");
            }

            byte[] headerSize = new byte[2]
            {
                (byte)(b64.Length / 256),
                (byte)(b64.Length % 256),
            };

            byte[] payload = headerSize.Concat(b64).ToArray();

            return payload;
        }

        /// <summary>
        /// Reads the KES header of stream and skips the header area.
        /// </summary>
        /// <param name="stream">
        /// The data stream.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="Header"/> class.
        /// </returns>
        public static T ReadHeader<T>(Stream stream)
            where T : Header, new()
        {
            stream.Seek(0, SeekOrigin.Begin);

            int headerSize = (stream.ReadByte() * 256) + stream.ReadByte();

            if (headerSize == 0)
            {
                return new T()
                {
                    Verbosity = HeaderVerbosity.Empty
                };
            }

            byte[] buffer = new byte[headerSize];
            stream.Read(buffer, 0, headerSize);

            T header = ReadJson<T>(buffer.Base64DecodeToString());

            int detail = 0;
            if (header.Version != null && header.EngineVersion != null)
            {
                detail++;

                if (header.BlockSize > 0)
                {
                    detail++;

                    if (header.Configuration != null)
                    {
                        detail++;
                    }
                }
            }

            header.Verbosity = (HeaderVerbosity)detail;

            stream.Seek(2 + headerSize, SeekOrigin.Begin);

            return header;
        }

        private static readonly JsonSerializerOptions jOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

#if NET6_0_OR_GREATER
        private static string ToJson(Header obj) => JsonWorker.ToJson(obj, obj.JsonSerializerContext);

        private static T ReadJson<T>(string json)
            where T : Header, new() => JsonWorker.ReadJson<T>(json, new T().JsonSerializerContext);
#else
        private static string ToJson(Header obj) => JsonWorker.ToJson(obj, jOptions);

        private static T ReadJson<T>(string json)
            where T : Header, new() => JsonWorker.ReadJson<T>(json, jOptions);
#endif
    }

#if NET6_0_OR_GREATER
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(Header))]
    internal partial class SourceGenerationHeaderContext : JsonSerializerContext
    {
    }
#endif
}
