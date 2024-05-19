using System.Security.Cryptography;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents methods to compute hashes and other crypto tranforms.
    /// </summary>
    public static class CryptoExtensions
    {
        /// <summary>
        /// Computes the SHA256 hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] Sha256(this byte[] buffer)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// Computes the SHA384 hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] Sha384(this byte[] buffer)
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                return sha384.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// Computes the SHA512 hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] Sha512(this byte[] buffer)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                return sha512.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// Computes the HMAC SHA256 hash value for the specified byte array with specified key value.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="key">The secret key.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] HmacSha256(this byte[] buffer, byte[] key)
        {
            using (var hmacSha256 = new HMACSHA256(key))
            {
                return hmacSha256.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// Computes the HMAC SHA384 hash value for the specified byte array with specified key value.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="key">The secret key.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] HmacSha384(this byte[] buffer, byte[] key)
        {
            using (var hmacSha384 = new HMACSHA384(key))
            {
                return hmacSha384.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// Computes the HMAC SHA512 hash value for the specified byte array with specified key value.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="key">The secret key.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] HmacSha512(this byte[] buffer, byte[] key)
        {
            using (var hmacSha512 = new HMACSHA512(key))
            {
                return hmacSha512.ComputeHash(buffer);
            }
        }
    }
}
