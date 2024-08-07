using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        /// Starts the task asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);
    }
}
