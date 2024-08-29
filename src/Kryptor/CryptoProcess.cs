namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Holds data about the crypto process.
    /// </summary>
    public struct CryptoProcess
    {
        private bool _initialized;

        /// <summary>
        /// Gets or sets the index of the chunk being processed.
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the block being processed.
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// Gets or sets the size of the block being processed.
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        /// Gets or sets the SHA256 hash of the block being processed.
        /// </summary>
        public byte[] BlockHash { get; set; }

        /// <summary>
        /// Gets custom data stored by crypto processors for this block.
        /// </summary>
        public Dictionary<string, object> BlockData { get; private set; }

        /// <summary>
        /// Gets custom data stored by crypto processors for the entire process.
        /// </summary>
        public Dictionary<string, object> ProcessData { get; private set; }

        /// <summary>
        /// Initializes all properties.
        /// </summary>
        public void InitializeData()
        {
            if (!_initialized)
            {
                BlockIndex = 0;
                BlockHash = Array.Empty<byte>();

                ChunkIndex = 0;

                BlockData = new Dictionary<string, object>();
                ProcessData = new Dictionary<string, object>();

                _initialized = true;
            }
        }

        internal void NextBlock(bool resetChunk)
        {
            BlockIndex++;

            BlockData.Clear();
            BlockHash = Array.Empty<byte>();

            if (resetChunk)
            {
                ChunkIndex = 0;
            }
        }
    }
}
