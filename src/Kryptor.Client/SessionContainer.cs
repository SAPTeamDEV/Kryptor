using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents container to store sessions.
    /// </summary>
    public class SessionContainer
    {
        private ISessionHost _sessionHost;
        private readonly int maxRunningSessions;
        private readonly Dictionary<int, SessionHolder> SessionPool = new Dictionary<int, SessionHolder>();
        private readonly List<Task> TaskPool = new List<Task>();
        private ISession[] sessions;
        private Task[] tasks;
        private CancellationTokenSource[] tokenSources;
        private ICollection<SessionHolder> holders;

        /// <summary>
        /// Gets an array of all sessions.
        /// </summary>
        public ISession[] Sessions
        {
            get
            {
                if (sessions == null)
                {
                    sessions = SessionPool.Values.Select(x => x.Session).ToArray();
                }

                return sessions;
            }
        }

        /// <summary>
        /// Gets an array of all tasks.
        /// </summary>
        public Task[] Tasks
        {
            get
            {
                if (tasks == null)
                {
                    tasks = SessionPool.Values.Select(x => x.Task).Concat(TaskPool).Where(x => x != null).ToArray();
                }

                return tasks;
            }
        }

        /// <summary>
        /// Gets an array of all cancellation tokens.
        /// </summary>
        public CancellationTokenSource[] TokenSources
        {
            get
            {
                if (tokenSources == null)
                {
                    tokenSources = SessionPool.Values.Select(x => x.TokenSource).ToArray();
                }

                return tokenSources;
            }
        }

        /// <summary>
        /// Gets all session holders.
        /// </summary>
        public ICollection<SessionHolder> Holders
        {
            get
            {
                if (holders == null)
                {
                    holders = SessionPool.Values;
                }

                return holders;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionContainer"/> class.
        /// </summary>
        /// <param name="sessionHost">
        /// The parent session host.
        /// </param>
        /// <param name="maxRunningSessions">
        /// The maximum allowed running sessions.
        /// </param>
        public SessionContainer(ISessionHost sessionHost, int maxRunningSessions)
        {
            _sessionHost = sessionHost;
            this.maxRunningSessions = maxRunningSessions;
        }

        /// <summary>
        /// Adds new session to the container.
        /// </summary>
        /// <returns>The unique identifier of this entry.</returns>
        public int Add(SessionHolder sessionHolder)
        {
            int rn = SessionPool.Keys.LastOrDefault() + 1;

            sessionHolder.Id = rn;
            SessionPool[rn] = sessionHolder;
            ResetCache();
            return rn;
        }

        /// <summary>
        /// Adds just a task to <see cref="Tasks"/>.
        /// </summary>
        /// <param name="task">
        /// The task to be added.
        /// </param>
        public void AddMonitoringTask(Task task)
        {
            TaskPool.Add(task);
            ResetCache();
        }

        /// <summary>
        /// Removes an entry from the container.
        /// </summary>
        /// <param name="id">
        /// The unique identifier returned by Add call.
        /// </param>
        public void Remove(int id)
        {
            SessionPool.Remove(id);
            ResetCache();
        }

        /// <summary>
        /// Waits until all sessions have been ended.
        /// </summary>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public async Task WaitAll(CancellationToken cancellationToken)
        {
            while (!Sessions.All(x => x.Status == SessionStatus.Ended))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    foreach (CancellationTokenSource token in TokenSources)
                    {
                        token.Cancel();
                    }
                }

                await Task.Delay(5);
            }

            cancellationToken.ThrowIfCancellationRequested();
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
            if (session.Status != SessionStatus.NotStarted)
            {
                throw new ArgumentException("The session is already started.");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            SessionHolder sessionHolder = new SessionHolder(session, tokenSource)
            {
                AutoRemove = autoRemove
            };

            Add(sessionHolder);
            StartQueuedSessions();
        }

        /// <summary>
        /// Starts registered sessions in a managed way.
        /// </summary>
        public void StartQueuedSessions()
        {
            IEnumerable<SessionHolder> running = Holders.Where(x => x.Session.Status == SessionStatus.Running);
            IEnumerable<SessionHolder> waiting = Holders.Where(x => x.Session.Status == SessionStatus.NotStarted && x.Session.IsReady(x.TokenSource.Token));

            if (waiting.Count() == 0 || running.Count() >= maxRunningSessions) return;

            int toBeStarted = Math.Min(maxRunningSessions - running.Count(), waiting.Count());
            for (int i = 0; i < toBeStarted; i++)
            {
                SessionHolder sessionHolder = waiting.ElementAt(i);
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

        private void ResetCache()
        {
            sessions = null;
            tasks = null;
            tokenSources = null;
            holders = null;
        }
    }
}
