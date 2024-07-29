using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents standard to generate random output.
    /// </summary>
    public interface IGenerator : IProgressReport
    {
        /// <summary>
        /// Fills the buffer with random values.
        /// </summary>
        /// <param name="buffer">
        /// The output buffer.
        /// </param>
        void Generate(byte[] buffer);
    }
}
