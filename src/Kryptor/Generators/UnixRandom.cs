using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using (var file = File.OpenRead("/dev/random"))
            {
                file.Read(buffer, 0, buffer.Length);
            }

            OnProgress?.Invoke(100);
        }
    }
}
