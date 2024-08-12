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
        /// Gets the Hidden status of this session.
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
        /// Gets the dependency list of the session. This session only starts when all of dependency sessions where completed successfully.
        /// </summary>
        List<ISession> Dependencies { get; }

        /// <summary>
        /// Gets the message queue of the session. All message will be handled and shown by client application.
        /// </summary>
        List<string> Messages { get; }

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
        /// Asks from session whether it could be started right now.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <returns><see langword="true"/> if the session has been ready and <see langword="false"/> when it's not ready.</returns>
        bool IsReady(CancellationToken cancellationToken);
    }
}
