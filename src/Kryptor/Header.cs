using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

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
        /// Gets or sets the level of details included in this header.
        /// </summary>
        public HeaderDetails DetailLevel { get; set; }

        /// <summary>
        /// Gets or sets the crypto provider.
        /// </summary>
        public CryptoTypes CryptoType { get; set; }

        /// <summary>
        /// Gets or sets the fingerprint of encrypted file.
        /// </summary>
        public byte[] Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets the file block size.
        /// </summary>
        public int? BlockSize { get; set; } = null;

        /// <summary>
        /// Gets or sets the configuration of continuous method.
        /// </summary>
        public bool? Continuous { get; set; } = null;

        /// <summary>
        /// Gets or sets the configuration of remove hash feature.
        /// </summary>
        public bool? RemoveHash { get; set; } = null;

        public bool? DynamicBlockProccessing { get; set; } = null;

        /// <summary>
        /// Gets or sets the extra header entries.
        /// </summary>
        public Dictionary<string, string> Extra { get; set; }

        internal byte[] CreatePayload()
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
                    var ts = buffer.LocatePattern(StartHeaderPattern);
                    if (ts != -1)
                    {
                        startPos = ts + StartHeaderPattern.Length;
                        startIndex = tries - 1;
                    }
                }

                if (startPos > -1 && endPos == -1)
                {
                    var te = buffer.LocatePattern(EndHeaderPattern);
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
                        DetailLevel = HeaderDetails.Empty
                    };
                }

                if (startPos > -1 && endPos > -1)
                {
                    break;
                }
            }

            stream.Seek(startPos, SeekOrigin.Begin);

            var dataBuffer = new byte[endPos - startPos];
            stream.Read(dataBuffer, 0, dataBuffer.Length);

            T header = ReadJson<T>(dataBuffer.Base64DecodeToString());

            int detail = 0;
            if (header.Version != null && header.EngineVersion != null)
            {
                detail++;

                if (header.Fingerprint != null)
                {
                    detail++;

                    if (header.BlockSize != null && header.Continuous != null && header.RemoveHash != null)
                    {
                        detail++;
                    }
                }
            }

            header.DetailLevel = (HeaderDetails)detail;

            stream.Seek(endPos + EndHeaderPattern.Length, SeekOrigin.Begin);

            return header;
        }

        private static JsonSerializerSettings jSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, jSettings);
        }

        private static T ReadJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, jSettings);
        }
    }
}
