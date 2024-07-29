
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
    /// Represents abstraction to implement sessions.
    /// </summary>
    public abstract class Session : ISession
    {
        /// <inheritdoc/>
        public abstract double Progress { get; protected set; }

        /// <inheritdoc/>
        public abstract string Description { get; protected set; }

        /// <inheritdoc/>
        public SessionStatus Status { get; protected set; }

        /// <inheritdoc/>
        public SessionEndReason EndReason { get; protected set; }

        /// <inheritdoc/>
        public Exception Exception { get; protected set; }

        /// <inheritdoc/>
        public Stopwatch Timer { get; protected set; }

        /// <summary>
        /// Sets all session properties to thir default data.
        /// </summary>
        protected Session()
        {
            Progress = 0;
            Description = "";

            Status = SessionStatus.NotStarted;
            EndReason = SessionEndReason.None;
            Exception = null;

            Timer = new Stopwatch();
        }

        /// <inheritdoc/>
        public async virtual Task StartAsync(CancellationToken cancellationToken)
        {
            Status = SessionStatus.Running;
            Timer.Start();
        }
    }
}
