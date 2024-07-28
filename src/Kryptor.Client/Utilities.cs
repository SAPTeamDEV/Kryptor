using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Gets a new non-repetitive file name
        /// </summary>
        /// <param name="source">
        /// The source file woth path.
        /// </param>
        /// <param name="newName">
        /// The intended name for new file.
        /// </param>
        /// <returns></returns>
        public static string GetNewFileName(string source, string newName)
        {
            string destination = Path.Combine(Directory.GetParent(source).FullName, newName);
            int suffix = 2;

            while (File.Exists(destination))
            {
                string tempName = $"{Path.GetFileNameWithoutExtension(destination)} ({suffix++}){Path.GetExtension(destination)}";

                if (!File.Exists(Path.Combine(Directory.GetParent(source).FullName, tempName)))
                {
                    destination = Path.Combine(Directory.GetParent(source).FullName, tempName);
                }
            }

            return destination;
        }

        /// <summary>
        /// Calculates the remaining time based on progress and elapsed milliseconds.
        /// </summary>
        /// <param name="progress">
        /// The passed progress.
        /// </param>
        /// <param name="elapsedMilliseconds">
        /// The elapsed time since progress start time.
        /// </param>
        /// <returns></returns>
        public static TimeSpan CalculateRemainingTime(double progress, long elapsedMilliseconds)
        {
            var rProg = Math.Round(progress);
            var remTime = progress > 0 ? (elapsedMilliseconds / progress) * (100 - progress) : 0;
            return TimeSpan.FromMilliseconds((int)remTime);
        }
    }
}
