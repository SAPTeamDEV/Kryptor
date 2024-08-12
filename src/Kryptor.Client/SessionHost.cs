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
        private bool _ending;

        /// <summary>
        /// Gets the session container.
        /// </summary>
        protected SessionContainer Container { get; }

        /// <summary>
        /// Gets the maximum allowed running sessions.
        /// </summary>
        public virtual int MaxRunningSessions { get; } = Environment.ProcessorCount - 1;

        /// <summary>
        /// Gets the special cancellation token to use in non-standard codes. (Blocking codes outside of sessions)
        /// </summary>
        protected CancellationTokenSource MasterToken { get; }

        /// <summary>
        /// Initializes all <see cref="SessionHost"/> instances.
        /// </summary>
        protected SessionHost()
        {
            Container = new SessionContainer(MaxRunningSessions);
            MasterToken = new CancellationTokenSource();
        }

        /// <inheritdoc/>
        public abstract void Start();

        /// <inheritdoc/>
        public virtual void End(bool cancelled)
        {
            if (_ending)
            {
                return;
            }
            _ending = true;

            if (cancelled)
            {
                var vts = Container.TokenSources.Where(x => !x.IsCancellationRequested);

                foreach (var token in vts)
                {
                    token.Cancel();
                }

                MasterToken.Cancel();

                // Tell to the SessionManager to do whatever needed
                Container.StartQueuedSessions();
            }

            Task.WaitAll(Container.Tasks);
        }

        /// <inheritdoc/>
        public void NewSession(ISession session, bool autoRemove = false)
        {
            Container.NewSession(session, autoRemove);
        }

        /// <inheritdoc/>
        public void MonitorTask(Task task)
        {
            Container.AddMonitoringTask(task);
        }
    }
}
