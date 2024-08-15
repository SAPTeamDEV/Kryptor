using System;
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

        /// <inheritdoc/>
        public bool Verbose { get; protected set; }

        /// <summary>
        /// Gets the special cancellation token to use in non-standard codes. (Blocking codes outside of sessions)
        /// </summary>
        protected CancellationTokenSource MasterToken { get; }

        /// <summary>
        /// Initializes all <see cref="SessionHost"/> instances.
        /// </summary>
        protected SessionHost()
        {
            Container = new SessionContainer(this, Environment.ProcessorCount - 1);
            MasterToken = new CancellationTokenSource();
        }

        /// <inheritdoc/>
        public abstract void Start(ClientContext context);

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
                Container.Cancel();

                MasterToken.Cancel();

                // Tell to the SessionManager to do whatever needed
                Container.StartQueuedSessions();
            }

            Container.WaitForRunningTasks();
        }

        /// <inheritdoc/>
        public virtual void NewSession(ISession session, bool autoRemove = false) => Container.NewSession(session, autoRemove);

        /// <inheritdoc/>
        public virtual void MonitorTask(Task task) => Container.AddMonitoringTask(task);

        /// <inheritdoc/>
        public abstract Task<TResponse> OnSessionRequest<TResponse>(ISession session, SessionRequest<TResponse> request, CancellationToken cancellationToken);
    }
}
