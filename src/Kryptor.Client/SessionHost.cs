using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents abstraction for session hosts.
    /// </summary>
    public abstract class SessionHost : ISessionHost
    {
        /// <summary>
        /// Gets the session container.
        /// </summary>
        protected SessionContainer Container { get; }

        /// <summary>
        /// Gets the maximum allowed running sessions.
        /// </summary>
        public virtual int MaxRunningSessions { get; } = Environment.ProcessorCount - 1;

        /// <summary>
        /// Initializes all <see cref="SessionHost"/> instances.
        /// </summary>
        protected SessionHost()
        {
            Container = new SessionContainer(MaxRunningSessions);
        }

        /// <inheritdoc/>
        public abstract void Start();

        /// <inheritdoc/>
        public virtual void End(bool cancelled)
        {
            if (cancelled)
            {
                var vts = Container.TokenSources.Where(x => !x.IsCancellationRequested);

                foreach (var token in vts)
                {
                    token.Cancel();
                }
            }

            Task.WaitAll(Container.Tasks);
        }

        /// <inheritdoc/>
        public void NewSession(ISession session, bool autoRemove = false)
        {
            Container.NewSession(session, autoRemove);
        }

        /// <summary>
        /// Adds given task to the task pool and be monitored by the session host.
        /// </summary>
        /// <param name="task">
        /// The task to be monitored.
        /// </param>
        public void MonitorTask(Task task)
        {
            Container.AddMonitoringTask(task);
        }
    }
}
