using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the data header.
    /// </summary>
    public class Header
    {
        static readonly byte[] StartHeaderPattern = new byte[] { 2, 97, 7, 64, 159, 37, 46, 128 };
        static readonly byte[] EndHeaderPattern = new byte[] { 97, 7, 64, 159, 37, 46, 128, 3 };

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
        /// Gets or sets the original name of file.
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// Gets or sets the fingerprint of encrypted file.
        /// </summary>
        public byte[] Fingerprint {  get; set; }

        /// <summary>
        /// Gets or sets the file block size.
        /// </summary>
        public int BlockSize {  get; set; }

        /// <summary>
        /// Gets or sets the configuration of continuous method.
        /// </summary>
        public bool Continuous {  get; set; }

        /// <summary>
        /// Gets or sets the extra header entries.
        /// </summary>
        public Dictionary<string, string> Extra { get; set; }

        internal byte[] CreatePayload()
        {
            string js = this.ToJson();
            byte[] b64 = js.Base64EncodeToByte();
            byte[] payload = StartHeaderPattern.Concat(b64)
                                               .Concat(EndHeaderPattern)
                                               .ToArray();

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
        public static Header ReadHeader(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[Math.Min(1024, stream.Length)];

            int startPos = -1;
            int endPos = -1;
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
                    }
                }

                if (endPos == -1)
                {
                    var te = buffer.LocatePattern(EndHeaderPattern);
                    if (te != -1)
                    {
                        endPos = te;
                    }
                }

                if ((startPos == -1 && tries > 5) || (endPos == -1 && tries > 10))
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    return new Header();
                }

                if (startPos > -1 && endPos > -1)
                {
                    break;
                }
            }
            
            stream.Seek(startPos, SeekOrigin.Begin);

            var dataBuffer = new byte[endPos - startPos];
            stream.Read(dataBuffer, 0, dataBuffer.Length);
            Header header = dataBuffer.Base64DecodeToString().ReadJson<Header>();

            stream.Seek(endPos + EndHeaderPattern.Length, SeekOrigin.Begin);

            return header;
        }
    }
}
