using System.Security.Cryptography;

namespace SAPTeam.Kryptor.Extensions
{
    /// <summary>
    /// Represents methods to compute hashes and other crypto tranforms.
    /// </summary>
    public static class CryptoExtensions
    {
        /// <summary>
        /// Computes the SHA256 hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for it.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] Sha256(this byte[] buffer)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// Computes the SHA256 hash value for the specified stream.
        /// </summary>
        /// <param name="stream">The input to compute the hash code for it.</param>
        /// <param name="startFromOrigin">If set to <see langword="true"/>, the stream will be moved to the beginning and at the end will be moved to the previous position.</param>
        /// <returns>The computed hash array.</returns>
        public static byte[] Sha256(this Stream stream, bool startFromOrigin = true)
        {
            long curPos = stream.Position;

            if (startFromOrigin && stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            byte[] hash;

            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(stream);
            }

            if (startFromOrigin && stream.CanSeek)
            {
                stream.Seek(curPos, SeekOrigin.Begin);
            }

            return hash;
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
            using (HMACSHA256 hmacSha256 = new HMACSHA256(key))
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
            using (HMACSHA384 hmacSha384 = new HMACSHA384(key))
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
            using (HMACSHA512 hmacSha512 = new HMACSHA512(key))
            {
                return hmacSha512.ComputeHash(buffer);
            }
        }
    }
}
