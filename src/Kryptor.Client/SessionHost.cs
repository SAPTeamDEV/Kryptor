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
        /// Get the session container.
        /// </summary>
        protected SessionContainer Container { get; } = new SessionContainer();

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
        public Task NewSession(ISession session, bool autoRemove = false)
        {
            if (session.Status != SessionStatus.NotStarted)
            {
                throw new ArgumentException("The session is already started.");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task task = session.StartAsync(tokenSource.Token);

            int id = Container.Add(session, task, tokenSource);

            if (autoRemove)
            {
                task.ContinueWith((x) => Container.Remove(id));
            }

            return task;
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
