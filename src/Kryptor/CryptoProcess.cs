using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Holds data about the crypto process.
    /// </summary>
    public struct CryptoProcess
    {
        /// <summary>
         /// Gets or sets the index of the chunk being processed.
         /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the block being processed.
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// Gets or sets the SHA256 hash of the block being processed.
        /// </summary>
        public byte[] BlockHash { get; set; }
    }
}
