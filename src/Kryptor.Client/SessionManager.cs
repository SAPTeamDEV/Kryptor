using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    public partial class SessionContainer
    {
        private readonly object _lock = new object();
        private readonly ISessionHost _sessionHost;
        private readonly int _maxRunningSessions;
        private bool _cancellationRequested;

        /// <summary>
        /// Waits until all sessions have been ended.
        /// </summary>
        /// <remarks>
        /// This method also waits for sessions that are not started yet.
        /// If the <paramref name="cancellationToken"/> was cancelled, this method will cancel all existing tokens in this container and waits for all of them to end.
        /// This method intended to be used by sub-session models. in standard session host based model, the monitoring, cancelling and ending is managed by various parts.
        /// But in sub-session models, you could achieve all of these features in this method.
        /// Using this method in session host is not recommended at all because this method is not intended for that porpose.
        /// </remarks>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public async Task WaitAll(CancellationToken cancellationToken)
        {
            while (!Sessions.All(x => x.Status == SessionStatus.Ended))
            {
                if (cancellationToken.IsCancellationRequested && !_cancellationRequested)
                {
                    Cancel();

                    _cancellationRequested = true;
                }

                await Task.Delay(5);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Waits for all running tasks at the moment to end.
        /// </summary>
        /// <remarks>
        /// This method just waits for running tasks and does not care about waiting sessions (as they don't have a registered task).
        /// A Task must be registered by <see cref="AddMonitoringTask(Task)"/> that waits for not started sessions before calling this method.
        /// This method only intended to be used in <see cref="ISessionHost.End(bool)"/>.
        /// so using it in sub-session models is not recommended as it could be lead to unexpected session exit and losing control of sub-sessions.
        /// it's highly recommended to use <see cref="WaitAll(CancellationToken)"/> method in that situation.
        /// </remarks>
        public void WaitForRunningTasks() => Task.WaitAll(Tasks);

        /// <summary>
        /// Sends cancellation request to all cancellation tokens in this container.
        /// </summary>
        public void Cancel()
        {
            foreach (CancellationTokenSource token in TokenSources.Where(x => !x.IsCancellationRequested))
            {
                token.Cancel();
            }
        }

        /// <summary>
        /// Starts a new session in this container.
        /// </summary>
        /// <param name="session">
        /// A session with status <see cref="SessionStatus.NotStarted"/>.
        /// </param>
        /// <param name="autoRemove">
        /// Determines whether to automatically remove session after end.
        /// </param>
        public void NewSession(ISession session, bool autoRemove)
        {
            SessionHolder sessionHolder = WrapSession(session, autoRemove);

            Add(sessionHolder);
            StartQueuedSessions();
        }

        /// <summary>
        /// Wraps the given <paramref name="session"/> into a <see cref="SessionHolder"/>.
        /// </summary>
        /// <param name="session">
        /// A session with status <see cref="SessionStatus.NotStarted"/>.
        /// </param>
        /// <param name="autoRemove">
        /// Determines whether to automatically remove session after end.
        /// </param>
        /// <returns></returns>
        public static SessionHolder WrapSession(ISession session, bool autoRemove)
        {
            if (session.Status != SessionStatus.NotStarted)
            {
                throw new ArgumentException("The session is already started.");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            SessionHolder sessionHolder = new SessionHolder(session, tokenSource)
            {
                AutoRemove = autoRemove
            };

            return sessionHolder;
        }

        /// <summary>
        /// Starts registered sessions in a managed way.
        /// </summary>
        /// <remarks>
        /// This is the core of the Session based Application Model logic.
        /// This task scheduler logic works with checking the number of running sessions (sessions with status <see cref="SessionStatus.Running"/>)
        /// and checking the waiting sessions (sessions with status <see cref="SessionStatus.NotStarted"/> and be ready).
        /// The ready to run sessions determined by calling the <see cref="ISession.IsReady(CancellationToken)"/> method, if the return value is <see langword="true"/> the session will be added to the waiting list, otherwise it will be ignored and rechecked at the next run of the task scheduler.
        /// There is some important notes about this process.
        /// <para>
        /// The task scheduler only runs after each <see cref="NewSession(ISession, bool)"/>calls and session ends. so if there is two sessions and the second session does not get ready until first session end, the task scheduler won't try to run that session and it will stuck at waiting status.
        /// </para>
        /// <para>
        /// The <see cref="ISession.IsReady(CancellationToken)"/> should not throw ANY exceptions at all. it may break the scheduler.
        /// Although there is some layers to handle exceptions, but the <see cref="ISession.IsReady(CancellationToken)"/> is only allwed to response with <see langword="true"/> or <see langword="false"/>.
        /// </para>
        /// </remarks>
        public void StartQueuedSessions()
        {
            lock (_lock)
            {
                QueueProcessImpl();
            }
        }

        private void QueueProcessImpl()
        {
            IEnumerable<SessionHolder> running = Holders.Where(x => x.Session.Status == SessionStatus.Running);
            IEnumerable<SessionHolder> waiting = Holders.Where(x => x.Session.Status == SessionStatus.NotStarted && SafeIsRady(x));

            if (waiting.Count() == 0 || running.Count() >= _maxRunningSessions) return;

            int toBeStarted = Math.Min(_maxRunningSessions - running.Count(), waiting.Count());
            for (int i = 0; i < toBeStarted; i++)
            {
                try
                {
                    SessionHolder sessionHolder = waiting.ElementAt(i);
                    StartManagedSession(sessionHolder);
                }
                catch
                {
                    // Do not crash the SessionManager for ANY reasons. it may cause unexpected behavior and infinite loops.
                    if (_sessionHost.Verbose)
                    {
                        throw;
                    }
                }
            }
        }

        private static bool SafeIsRady(SessionHolder holder)
        {
            bool ready;

            try
            {
                ready = holder.Session.IsReady(holder.TokenSource.Token);
            }
            catch
            {
                ready = false;
            }

            return ready;
        }

        private void StartManagedSession(SessionHolder sessionHolder)
        {
            Task task = sessionHolder.StartTask(_sessionHost, false);
            if (task != null)
            {
                task.ContinueWith(x => StartQueuedSessions());

                if (sessionHolder.AutoRemove)
                {
                    task.ContinueWith(x => Remove(sessionHolder.Id));
                }
            }
        }
    }
}
