using System;
using System.IO;

namespace SAPTeam.Kryptor.Generators
{
    /// <summary>
    /// Generates random data with unix standard /dev/random character device.
    /// </summary>
    public class UnixRandom : IGenerator
    {
        /// <inheritdoc/>
        public event Action<double> OnProgress;

        /// <inheritdoc/>
        public void Generate(byte[] buffer)
        {
            using (FileStream file = File.OpenRead("/dev/random"))
            {
                file.Read(buffer, 0, buffer.Length);
            }

            OnProgress?.Invoke(100);
        }
    }
}
