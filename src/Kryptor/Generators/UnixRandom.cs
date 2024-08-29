using System.Runtime.InteropServices;

namespace SAPTeam.Kryptor.Generators
{
    /// <summary>
    /// Generates random data with unix standard /dev/random character device.
    /// </summary>
    public class UnixRandom : IGenerator
    {
        /// <inheritdoc/>
        public event EventHandler<double> ProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixRandom"/> class.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public UnixRandom()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !File.Exists("/dev/random"))
            {
                throw new PlatformNotSupportedException();
            }
        }

        /// <inheritdoc/>
        public void Generate(byte[] buffer)
        {
            ProgressChanged?.Invoke(this, -1);

            using (FileStream file = File.OpenRead("/dev/random"))
            {
                file.Read(buffer, 0, buffer.Length);
            }

            ProgressChanged?.Invoke(this, 100);
        }
    }
}
