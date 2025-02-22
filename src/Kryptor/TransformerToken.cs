﻿using System.Text.RegularExpressions;

using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Parses and holds transformer token data.
    /// </summary>
    public struct TransformerToken
    {
        private static readonly string pattern = @"^(?<TransformerName>\w+):(?<SecretKey>\w+):(?<KeySize>\d+)(?::(?<Salt>\w+))?(?:-(?<Rotate>\d+))?$";
        private static readonly Regex regex = new Regex(pattern);

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
        public static TransformerToken Parse(string token)
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

        /// <summary>
        /// Determines whether the given token is valid.
        /// </summary>
        /// <param name="token">
        /// The token to test.
        /// </param>
        /// <returns></returns>
        public static bool IsValid(string token)
        {
            Match match = regex.Match(token);

            return match.Success;
        }

        /// <summary>
        /// Determines whether this token is valid.
        /// </summary>
        /// <returns></returns>
        public readonly bool IsValid() => TransformerName != null && SecretKey != null && KeySize > 0;

        /// <summary>
        /// Initializes a new transformer with this token.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public readonly ITransformer GetTransformer() => Transformers.GetTransformer(this);

        /// <summary>
        /// Generates keystore based on this token data.
        /// </summary>
        /// <param name="margin">
        /// The requested extra size to make the keystore even more secure.
        /// </param>
        /// <returns>
        /// A new <see cref="KeyStore"/> initialized with this token data.
        /// </returns>
        public readonly KeyStore GenerateKeyStore(int margin)
        {
            ITransformer transformer = GetTransformer();

            byte[] buffer = new byte[(KeySize * 32) + margin];
            transformer.Generate(buffer, Rotate);

            return new KeyStore(buffer);
        }
    }
}
