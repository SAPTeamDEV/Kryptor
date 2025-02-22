﻿using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Represents standard to transform specified inputs to a reproducible output.
    /// </summary>
    public interface ITransformer : IProgressReport
    {
        /// <summary>
        /// Fills the buffer with generated values.
        /// </summary>
        /// <param name="buffer">
        /// The output buffer.
        /// </param>
        /// <param name="rotate">
        /// The number of changes applies to buffer after filling.
        /// </param>
        void Generate(byte[] buffer, int rotate);
    }
}
