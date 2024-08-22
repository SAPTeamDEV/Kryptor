using System;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents a unified header for kryptor front ends.
    /// </summary>
    public class ClientHeader : Header
    {
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
    }
}
