using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;


#if NET6_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif

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
        /// Exports the header contents as base64 encoded bytes array.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OverflowException"></exception>
        public byte[] CreatePayload()
        {
            string js = ToJson(this);
            byte[] b64 = js.Base64EncodeToByte();
            byte[] payload = StartHeaderPattern.Concat(b64)
                                               .Concat(EndHeaderPattern)
                                               .ToArray();

            return payload.Length > 8192
                ? throw new OverflowException($"Header payload size is out of allowed range {payload} > 8192")
                : payload;
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
            stream.Read(dataBuffer, 0, dataBuffer.Length);

            T header = ReadJson<T>(dataBuffer.Base64DecodeToString());

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

            stream.Seek(endPos + EndHeaderPattern.Length, SeekOrigin.Begin);

            return header;
        }

#if NET6_0_OR_GREATER
        static readonly JsonSerializerOptions jOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        private static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, jOptions);
        }

        private static T ReadJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, jOptions);
        }
#else
        private static readonly JsonSerializerSettings jSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private static string ToJson(object obj) => JsonConvert.SerializeObject(obj, jSettings);

        private static T ReadJson<T>(string json) => JsonConvert.DeserializeObject<T>(json, jSettings);
#endif
    }
}
