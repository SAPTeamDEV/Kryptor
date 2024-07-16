namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the level of details included in header.
    /// </summary>
    public enum HeaderVerbosity
    {
        /// <summary>
        /// Indicates an empty header.
        /// </summary>
        Empty = 0,

        /// <summary>
        /// The lowest detail level, Only includes api version and engine version.
        /// </summary>
        Minimum = 1,

        /// <summary>
        /// The Medium detail level, Includes block size besides the minimum level.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// The Highest detail level, Includes all parameters in yje header.
        /// </summary>
        Maximum = 3,
    }
}
