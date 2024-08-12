using System;

namespace SAPTeam.Kryptor.Helpers
{
    /// <summary>
    /// Represents interface to report progress of works.
    /// </summary>
    public interface IProgressReport
    {
        /// <summary>
        /// Called when a part of work is done.
        /// </summary>
        event Action<double> OnProgress;
    }
}
