using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Parses and holds transformer token data.
    /// </summary>
    public struct TransformerToken
    {
        static readonly string pattern = @"^(?<TransformerName>\w+):(?<SecretKey>\w+):(?<KeySize>\d+)(?::(?<Salt>\w+))?(?:-(?<Rotate>\d+))?$";

        static readonly Regex regex = new Regex(pattern);

        /// <summary>
        /// Gets or sets the transformer name.
        /// </summary>
        public string TransformerName { get; set; }

        /// <summary>
        /// Gets or sets the secret key.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the key size.
        /// </summary>
        public int KeySize { get; set; }

        /// <summary>
        /// Gets or sets the salt value used for altering seed and outlut data.
        /// </summary>
        public byte[] Salt { get; set; }

        /// <summary>
        /// Gets or sets the number of additional output transformations.
        /// </summary>
        public int Rotate { get; set; }

        /// <summary>
        /// Parses input string to a transformer token.
        /// </summary>
        /// <param name="token">
        /// The input string with valid transformer token format.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="TransformerToken"/>.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        static public TransformerToken Parse(string token)
        {
            Match match = regex.Match(token);

            if (match.Success)
            {
                // Extract the named groups
                string transformerName = match.Groups["TransformerName"].Value.Trim().ToLower();
                byte[] secretKey = match.Groups["SecretKey"].Value.Trim().Encode();
                int keySize = int.Parse(match.Groups["KeySize"].Value.Trim());
                byte[] salt = !string.IsNullOrEmpty(match.Groups["Salt"].Value.Trim()) ? match.Groups["Salt"].Value.Trim().Encode() : null;
                int rotate = !string.IsNullOrEmpty(match.Groups["Rotate"].Value.Trim()) ? int.Parse(match.Groups["Rotate"].Value.Trim()) : 0;

                return new TransformerToken()
                {
                    TransformerName = transformerName,
                    SecretKey = secretKey,
                    KeySize = keySize,
                    Salt = salt,
                    Rotate = rotate
                };
            }
            else
            {
                throw new ArgumentException("Input does not match the expected format.");
            }
        }
    }
}
