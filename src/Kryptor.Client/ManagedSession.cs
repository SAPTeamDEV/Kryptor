using System;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents an empty session to be managed by the session host.
    /// </summary>
    public class ManagedSession : Session
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedSession"/> class.
        /// </summary>
        public ManagedSession() : base() => Status = SessionStatus.Managed;

        /// <summary>
        /// Sets a new value for session's progress.
        /// </summary>
        /// <param name="progress">
        /// The new value from -1 to 100.
        /// </param>
        public void SetProgress(double progress) => Progress = progress;

        /// <summary>
        /// Sets a new value for session's description.
        /// </summary>
        /// <param name="description">
        /// The new description.
        /// </param>
        public void SetDescription(string description) => Description = description;

        /// <summary>
        /// Sets the session status to <see cref="SessionStatus.Ended"/> and sets the end reason and exception.
        /// </summary>
        /// <param name="endReason"></param>
        /// <param name="exception"></param>
        public void SetEndStatus(SessionEndReason endReason, Exception exception = null)
        {
            Status = SessionStatus.Ended;
            EndReason = endReason;
            Exception = exception;
        }

        /// <inheritdoc/>
        protected override Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
