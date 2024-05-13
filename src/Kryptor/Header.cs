using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the data header.
    /// </summary>
    public class Header
    {
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
    }
}
