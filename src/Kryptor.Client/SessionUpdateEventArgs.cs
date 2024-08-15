using System;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents class to hold session updates event data.
    /// </summary>
    public class SessionUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current progress value of the session.
        /// </summary>
        public double Progress { get; internal set; }

        /// <summary>
        /// Gets the current description value of the session.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets whether the session is paused.
        /// </summary>
        public bool IsPause { get; internal set; }
    }
}
