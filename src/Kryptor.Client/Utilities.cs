using System.Text.Json;
using System.Text.Json.Serialization;

using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides common utilities used by kryptor front ends.
    /// </summary>
    public static class Utilities
    {
        private static readonly JsonSerializerOptions jOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        /// <summary>
        /// Gets pre-initialized <see cref="JsonWorker"/> to work with Client defined types.
        /// </summary>
        public static JsonWorker ClientTypesJsonWorker { get; }

        static Utilities() =>
#if NET6_0_OR_GREATER
            ClientTypesJsonWorker = new JsonWorker(null, ClientTypesJsonSerializerContext.Default);
#else
            ClientTypesJsonWorker = new JsonWorker(jOptions, null);
#endif

        /// <summary>
        /// Gets the short string representation of the given version.
        /// </summary>
        /// <param name="verStr">Version string to process.</param>
        /// <returns></returns>
        public static string GetShortVersionString(string verStr)
        {
            Version ver = new Version(verStr);
            return ver.ToString(3);
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
        /// <param name="path">
        /// The destination directory.
        /// </param>
        /// <param name="newName">
        /// The proposed name for the new file.
        /// </param>
        /// <returns></returns>
        public static string GetNewFileName(string path, string newName)
        {
            string destination = Path.Combine(path, newName);
            int suffix = 2;

            while (File.Exists(destination))
            {
                string tempName = $"{Path.GetFileNameWithoutExtension(newName)} ({suffix++}){Path.GetExtension(newName)}";

                if (!File.Exists(Path.Combine(path, tempName)))
                {
                    destination = Path.Combine(path, tempName);
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

        /// <summary>
        /// Creates and returns a random directory in user's temp folder.
        /// </summary>
        /// <returns></returns>
        public static string CreateTempFolder()
        {
            string fPath;

            while (true)
            {
                fPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                if (!File.Exists(fPath) && !Directory.Exists(fPath))
                {
                    break;
                }
            }

            Directory.CreateDirectory(fPath);
            return fPath;
        }

        /// <summary>
        /// Creates the directory if it's not exists.
        /// </summary>
        /// <param name="path">
        /// The directory path.
        /// </param>
        /// <returns>
        /// Absolute path of the directory.
        /// </returns>
        public static string EnsureDirectoryExists(string path)
        {
            string absPath = Path.GetFullPath(path);

            if (!Directory.Exists(absPath))
            {
                Directory.CreateDirectory(absPath);
            }

            return absPath;
        }

        /// <summary>
        /// Creates the file if it's not exists.
        /// </summary>
        /// <param name="path">
        /// The file path.
        /// </param>
        /// <returns>
        /// Absolute path of the file.
        /// </returns>
        public static string EnsureFileExists(string path)
        {
            string absPath = Path.GetFullPath(path);

            if (!File.Exists(absPath))
            {
                File.Create(absPath).Dispose();
            }

            return absPath;
        }

        /// <summary>
        /// Performs XOR operation on two byte array.
        /// </summary>
        /// <param name="a1">
        /// The first array.
        /// </param>
        /// <param name="a2">
        /// The socond array.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] XOR(byte[] a1, byte[] a2)
        {
            if (a1.Length == a2.Length)
            {
                byte[] result = new byte[a1.Length];
                for (int i = 0; i < a1.Length; i++)
                {
                    result[i] = (byte)(a1[i] ^ a2[i]);
                }

                return result;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }

#if NET6_0_OR_GREATER
    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(WordlistVerificationMetadata))]
    [JsonSerializable(typeof(List<WordlistVerificationMetadata>))]
    [JsonSerializable(typeof(KeyChain))]
    [JsonSerializable(typeof(List<KeyChain>))]
    [JsonSerializable(typeof(WordlistIndex))]
    [JsonSerializable(typeof(WordlistIndexLegacy))]
    internal partial class ClientTypesJsonSerializerContext : JsonSerializerContext { }
#endif
}
