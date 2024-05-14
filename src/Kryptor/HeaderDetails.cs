namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the level of details included in header.
    /// </summary>
    public enum HeaderDetails
    {
        /// <summary>
        /// The lowest detail level, Only includes api version and engine version.
        /// </summary>
        Minimum = 0,

        /// <summary>
        /// The medium detail level, Includes key store fingerprint and original file name besides the minimum level.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// The Highest detail level, Includes all parameters in header.
        /// </summary>
        Maximum = 2,
    }
}
