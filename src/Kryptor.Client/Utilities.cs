using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides common utilities used by kryptor front ends.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Gets the short string representation of the given version.
        /// </summary>
        /// <param name="verStr">Version string to process.</param>
        /// <returns></returns>
        public static string GetShortVersionString(string verStr)
        {
            var ver = new Version(verStr);
            return string.Join(".", ver.Major, ver.Minor, ver.Build);
        }

        /// <summary>
        /// Generates keystore based on given token.
        /// </summary>
        /// <param name="token">
        /// Source token that has the required data to generate keystore.
        /// </param>
        /// <returns></returns>
        public static KeyStore GenerateKeyStoreFromToken(TransformerToken token)
        {
            ITranformer tranformer = Transformers.GetTranformer(token);
            byte[] buffer = new byte[token.KeySize * 32];
            tranformer.Generate(buffer, token.Rotate);
            return new KeyStore(buffer);
        }
    }
}
