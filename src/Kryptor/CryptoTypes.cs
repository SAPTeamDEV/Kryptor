namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Specifies the crypto provider type.
    /// </summary>
    public enum CryptoTypes
    {
        /// <summary>
        /// Invalid crypto provider.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Provides Standalone Key (SK) Crypto mechanism.
        /// In this way, each 31 bytes of data is encrypted with a different key Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
        /// </summary>
        SK = 1,

        /// <summary>
        /// Provides Transformed Key (TK) Crypto mechanism.
        /// In this way, each 31 bytes of data is encrypted with a mix-transformed key.
        /// </summary>
        TK = 2,

        /// <summary>
        /// Provides Mixed Vector (MV) Crypto mechanism.
        /// In this way, each 31 bytes of data is encrypted with a different key and a mixed iv Until all the keys are used, then it continues from the first key and this process continues until the end of encryption.
        /// </summary>
        MV = 3,

        /// <summary>
        /// Provides Transformed Parameters (TP) Crypto mechanism.
        /// In this way, each 31 bytes of data is encrypted with a mix-transformed key and iv.
        /// </summary>
        TP = 4,

        /// <summary>
        /// Provides Dynamic Encryption (DE) Crypto mechanism.
        /// In this way, each 31 bytes of data is encrypted with a dynamic key and iv generated with attention of all parameters and offers the highest security.
        /// </summary>
        DE = 5,
    }
}
