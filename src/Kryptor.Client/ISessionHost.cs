namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents interface to start and end program.
    /// </summary>
    public interface ISessionHost
    {
        /// <summary>
        /// Gets the verbosity of the session host.
        /// </summary>
        bool Verbose { get; }

        /// <summary>
        /// Starts the host session in the current thread.
        /// </summary>
        /// <param name="context">
        /// The caller context.
        /// </param>
        void Start(ClientContext context);

        /// <summary>
        /// Ends the host session.
        /// </summary>
        /// <param name="cancelled">
        /// Whether the host session ending triggered by cancel request.
        /// </param>
        void End(bool cancelled);

        /// <summary>
        /// Starts a new session in this session host.
        /// </summary>
        /// <param name="session">
        /// A session with status <see cref="SessionStatus.NotStarted"/>.
        /// </param>
        /// <param name="autoRemove">
        /// Determines whether to automatically remove session after end.
        /// </param>
        /// <param name="autoStart">
        /// Determines whether to automatically call the session manager to start this session.
        /// </param>
        void NewSession(ISession session, bool autoRemove, bool autoStart);

        /// <summary>
        /// Adds given task to the task pool and be monitored by the session host.
        /// </summary>
        /// <param name="task">
        /// The task to be monitored.
        /// </param>
        void MonitorTask(Task task);

        /// <summary>
        /// Handles requests from the sessions.
        /// </summary>
        /// <remarks>
        /// This method should send requests to user and if it's not possible, it could return the <see cref="SessionRequest{TResponse}.DefaultValue"/> or throws an exception.
        /// The thrown exception would be catched on the sender session and the session will fail.
        /// </remarks>
        /// <param name="session">
        /// The paused session.
        /// </param>
        /// <param name="request">
        /// The request data.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <typeparam name="TResponse">
        /// The type of requested response from user.
        /// </typeparam>
        /// <returns>
        /// The session host or user response.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This exception thrown when the requested <typeparamref name="TResponse"/> is not supported by this session host.
        /// </exception>
        Task<TResponse> OnSessionRequest<TResponse>(ISession session, SessionRequest<TResponse> request, CancellationToken cancellationToken);
    }
}
