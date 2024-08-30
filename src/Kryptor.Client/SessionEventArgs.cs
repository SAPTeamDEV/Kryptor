namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents class to hold session events data.
    /// </summary>
    public class SessionEventArgs : SessionUpdateEventArgs
    {
        /// <summary>
        /// Gets the value of the session status before triggering this event.
        /// </summary>
        public SessionStatus PreviousStatus { get; internal set; }

        /// <summary>
        /// Gets the current status of the session.
        /// </summary>
        public SessionStatus Status { get; internal set; }

        /// <summary>
        /// Gets the end reason of the session.
        /// </summary>
        public SessionEndReason EndReason { get; internal set; }

        /// <summary>
        /// Gets the exception value of the session.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Gets the current message queue of the session.
        /// </summary>
        public string[] Messages { get; internal set; }
    }
}
