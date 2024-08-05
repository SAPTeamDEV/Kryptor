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
        protected SessionContainer Container { get; } = new SessionContainer();

        /// <summary>
        /// Gets the maximum allowed running sessions.
        /// </summary>
        public int MaxRunningSessions { get; } = Environment.ProcessorCount - 1;

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
            if (session.Status != SessionStatus.NotStarted)
            {
                throw new ArgumentException("The session is already started.");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            SessionHolder sessionHolder = new SessionHolder(session, tokenSource);

            sessionHolder.AutoRemove = autoRemove;

            Container.Add(sessionHolder);
            StartQueuedSessions();
        }

        /// <summary>
        /// Starts registered sessions in a managed way.
        /// </summary>
        public void StartQueuedSessions()
        {
            var running = Container.Holders.Where(x => x.Session.Status == SessionStatus.Running);
            var waiting = Container.Holders.Where(x => x.Session.Status == SessionStatus.NotStarted);

            if (waiting.Count() == 0 || running.Count() >= MaxRunningSessions) return;

            int toBeStarted = Math.Min(MaxRunningSessions - running.Count(), waiting.Count());
            for (int i = 0; i < toBeStarted; i++)
            {
                var sessionHolder = waiting.ElementAt(i);
                var task = sessionHolder.StartTask(false);
                if (task != null)
                {
                    task.ContinueWith(x => StartQueuedSessions());

                    if (sessionHolder.AutoRemove)
                    {
                        task.ContinueWith(x => Container.Remove(sessionHolder.Id));
                    }
                }
            }
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
