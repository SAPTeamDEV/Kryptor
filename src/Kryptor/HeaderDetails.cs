namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the level of details included in header.
    /// </summary>
    public enum HeaderDetails
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
        /// The Medium detail level, Includes keystore fingerprint besides the minimum level.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// The Highest detail level, Includes all parameters in header.
        /// </summary>
        Maximum = 3,
    }
}
