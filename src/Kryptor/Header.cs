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
        private static readonly byte[] StartHeaderPattern = new byte[] { 2, 97, 7, 64, 159, 37, 46, 128 };
        private static readonly byte[] EndHeaderPattern = new byte[] { 97, 7, 64, 159, 37, 46, 128, 3 };

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
            byte[] b64 = ToBase64Json();

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
        /// Exports the header contents as base64 encoded bytes array with pre 0.20 method.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OverflowException"></exception>
        public byte[] ExportClassic()
        {
            byte[] b64 = ToBase64Json();

            byte[] payload = StartHeaderPattern.Concat(b64)
                                               .Concat(EndHeaderPattern)
                                               .ToArray();

            return payload.Length > 8192
                ? throw new OverflowException($"Header payload size is out of allowed range {payload} > 8192")
                : payload;
        }

        private byte[] ToBase64Json()
        {
            string js = ToJson(this);
            byte[] b64 = js.Base64EncodeToByte();
            return b64;
        }

        /// <summary>
        /// Reads the KES header of the stream and skips the header area.
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
#if NET8_0_OR_GREATER
            stream.ReadExactly(buffer, 0, headerSize);
#else
            stream.Read(buffer, 0, headerSize);
#endif

            T header;
            try
            {
                header = ReadHeaderInternal<T>(buffer.Base64DecodeToString());
            }
            catch
            {
                header = ReadHeaderClassic<T>(stream);
            }

            return header;
        }

        /// <summary>
        /// Reads the KES header of the stream and skips the header area with pre 0.20 method.
        /// </summary>
        /// <param name="stream">
        /// The data stream.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="Header"/> class.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2022:Avoid inexact read with 'Stream.Read'", Justification = "This behavior is intended")]
        public static T ReadHeaderClassic<T>(Stream stream)
            where T : Header, new()
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[Math.Min(1024, stream.Length)];

            int startPos = -1;
            int endPos = -1;
            int startIndex = 0;
            int tries = 0;

            while (true)
            {
                tries++;

                // Get 1KB of data to search for header.
                stream.Read(buffer, 0, buffer.Length);

                if (startPos == -1)
                {
                    int ts = buffer.LocatePattern(StartHeaderPattern);
                    if (ts != -1)
                    {
                        startPos = ts + StartHeaderPattern.Length;
                        startIndex = tries - 1;
                    }
                }

                if (startPos > -1 && endPos == -1)
                {
                    int te = buffer.LocatePattern(EndHeaderPattern);
                    if (te != -1)
                    {
                        endPos = te;
                    }
                }

                if ((startPos == -1 && tries > 2) || (endPos == -1 && tries > 8 + startIndex))
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    return new T()
                    {
                        Verbosity = HeaderVerbosity.Empty
                    };
                }

                if (startPos > -1 && endPos > -1)
                {
                    break;
                }
            }

            stream.Seek(startPos, SeekOrigin.Begin);

            byte[] dataBuffer = new byte[endPos - startPos];
#if NET8_0_OR_GREATER
            stream.ReadExactly(dataBuffer);
#else
            stream.Read(dataBuffer, 0, dataBuffer.Length);
#endif

            T header = ReadHeaderInternal<T>(dataBuffer.Base64DecodeToString());

            stream.Seek(endPos + EndHeaderPattern.Length, SeekOrigin.Begin);

            return header;
        }

        private static T ReadHeaderInternal<T>(string headerJson) where T : Header, new()
        {
            T header = ReadJson<T>(headerJson);

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
