namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents class to hold encryption data of files.
    /// </summary>
    public class KeyChain
    {
        /// <summary>
        /// Gets or sets the unique identifier of the encrypted file.
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Gets or sets the fingerprint of the encryptor keystore.
        /// </summary>
        public byte[] Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets the keystore hint (transformer token or file name).
        /// </summary>
        public string KeyStoreHint { get; set; }
    }
}
