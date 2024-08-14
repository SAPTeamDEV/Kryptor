using System;
using System.IO;

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
            Version ver = new Version(verStr);
            return string.Join(".", ver.Major, ver.Minor, ver.Build);
        }

        /// <summary>
        /// Generates keystore based on given token.
        /// </summary>
        /// <param name="token">
        /// Source token that has the required data to generate keystore.
        /// </param>
        /// <param name="progressReport">
        /// Called when a part of work is done.
        /// </param>
        /// <param name="margin">
        /// The requested extra size to make the keystore even more secure.
        /// </param>
        /// <returns></returns>
        public static KeyStore GenerateKeyStoreFromToken(TransformerToken token, EventHandler<double> progressReport, int margin = 0)
        {
            ITranformer transformer = Transformers.GetTranformer(token);
            transformer.ProgressChanged += progressReport;

            byte[] buffer = new byte[(token.KeySize * 32) + margin];
            transformer.Generate(buffer, token.Rotate);

            progressReport(transformer, -1);
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
        public static TimeSpan CalculateRemainingTimeSpan(double progress, long elapsedMilliseconds) => TimeSpan.FromMilliseconds((int)CalculateRemainingTime(progress, elapsedMilliseconds));

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
        public static double CalculateRemainingTime(double progress, long elapsedMilliseconds)
        {
            double rProg = Math.Round(progress);
            double remTime = progress > 0 ? elapsedMilliseconds / rProg * (100 - rProg) : 0;
            return remTime;
        }

        /// <summary>
        /// Converts given byte length to a human readable unit.
        /// </summary>
        /// <param name="bytes">
        /// The length in byte.
        /// </param>
        /// <returns></returns>
        public static string ConvertBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##}{sizes[order]}";
        }
    }
}
