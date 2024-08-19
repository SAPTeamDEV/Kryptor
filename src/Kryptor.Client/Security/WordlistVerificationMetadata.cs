namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents wordlist's metadata needed for verification.
    /// </summary>
    public class WordlistVerificationMetadata
    {
        /// <summary>
        /// The numeric id of the fragment.
        /// </summary>
        public int FragmentId { get; set; }

        /// <summary>
        /// The existing string in the fragment to verify the compatibility of the compiler.
        /// </summary>
        public string LookupString { get; set; }

        /// <summary>
        /// Checksum populated by XORing the file hash with the wordlist hash.
        /// </summary>
        public byte[] Checksum { get; set; }
    }
}
