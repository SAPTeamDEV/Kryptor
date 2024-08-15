using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents interface to start, monitor and control tasks.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Gets the user-friendly name of the session.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the progress percentage of this session.
        /// </summary>
        double Progress { get; }

        /// <summary>
        /// Gets the description of this session.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets whether this session is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets whether this session is paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Gets whether this session is hidden.
        /// </summary>
        bool IsHidden { get; }

        /// <summary>
        /// Gets the current status of the session
        /// </summary>
        SessionStatus Status { get; }

        /// <summary>
        /// Gets the reason of ending the session.
        /// </summary>
        SessionEndReason EndReason { get; }

        /// <summary>
        /// Gets the thrown exception during session run.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Gets the stopwatch for this session.
        /// </summary>
        Stopwatch Timer { get; }

        /// <summary>
        /// Gets the message queue of the session. All message will be handled and shown by client application.
        /// </summary>
        List<string> Messages { get; }

        /// <summary>
        /// Occurs when the session started by SessionManager.
        /// </summary>
        event EventHandler<SessionEventArgs> SessionStarted;

        /// <summary>
        /// Occurs when the progress value of the session is changed.
        /// </summary>
        event EventHandler<SessionUpdateEventArgs> ProgressChanged;

        /// <summary>
        /// Occurs when the description value is changed.
        /// </summary>
        event EventHandler<SessionUpdateEventArgs> DescriptionChanged;

        /// <summary>
        /// Occurs when the status of the session is changed.
        /// </summary>
        event EventHandler<SessionEventArgs> StatusChanged;

        /// <summary>
        /// Occurs when the session is ended.
        /// </summary>
        event EventHandler<SessionEventArgs> SessionEnded;

        /// <summary>
        /// Starts the task asynchronously.
        /// </summary>
        /// <param name="sessionHost">
        /// The parent session host.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <returns>
        /// A new <see cref="Task"/> representation of the session.
        /// </returns>
        Task StartAsync(ISessionHost sessionHost, CancellationToken cancellationToken);

        /// <summary>
        /// Sets this session as dependency for the given <paramref name="session"/>. The session waits for this session to successfully end.
        /// </summary>
        /// <param name="session">
        /// A session with status <see cref="SessionStatus.NotStarted"/>.
        /// </param>
        void ContinueWith(ISession session);

        /// <summary>
        /// Adds the given <paramref name="session"/> as dependency. this session won't start unless all dependecy session completed successfully. All dependencies must be set before starting the session otherwise it won't have any effects.
        /// </summary>
        /// <param name="session">
        /// The session to be added as dependency.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="session"/> is added to the dependency list and <see langword="false"/> if the <paramref name="session"/> is not added because this session already started or duplicated <paramref name="session"/>.
        /// </returns>
        bool AddDependency(ISession session);

        /// <summary>
        /// Asks from session whether it could be started right now.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <returns><see langword="true"/> if the session has been ready and <see langword="false"/> when it's not ready.</returns>
        bool IsReady(CancellationToken cancellationToken);

        /// <summary>
        /// Pauses this session and sends the request to the <paramref name="sessionHost"/> to process the request.
        /// </summary>
        /// <param name="sessionHost">
        /// The parent session host.
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
        /// The response of the end user.
        /// </returns>
        Task<TResponse> SendRequest<TResponse>(ISessionHost sessionHost, SessionRequest<TResponse> request, CancellationToken cancellationToken);
    }
}
