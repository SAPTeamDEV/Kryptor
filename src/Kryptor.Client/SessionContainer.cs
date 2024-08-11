using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents container to store sessions.
    /// </summary>
    public class SessionContainer
    {
        static CryptoRandom crng = new CryptoRandom();

        readonly int maxRunningSessions;

        readonly Dictionary<int, SessionHolder> SessionPool = new Dictionary<int, SessionHolder>();
        readonly List<Task> TaskPool = new List<Task>();

        ISession[] sessions;
        Task[] tasks;
        CancellationTokenSource[] tokenSources;
        ICollection<SessionHolder> holders;

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
        /// <param name="maxRunningSessions">
        /// The maximum allowed running sessions.
        /// </param>
        public SessionContainer(int maxRunningSessions)
        {
            this.maxRunningSessions = maxRunningSessions;
        }

        /// <summary>
        /// Adds new session to the container.
        /// </summary>
        /// <returns>The unique identifier of this entry.</returns>
        public int Add(SessionHolder sessionHolder)
        {
            int rn;
            while (true)
            {
                rn = crng.Next();
                if (!SessionPool.ContainsKey(rn))
                {
                    break;
                }
            }
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
        public async void WaitAll()
        {
            while (Sessions.All(x => x.Status == SessionStatus.Ended))
            {
                await Task.Delay(1);
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
            if (session.Status != SessionStatus.NotStarted)
            {
                throw new ArgumentException("The session is already started.");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            SessionHolder sessionHolder = new SessionHolder(session, tokenSource);

            sessionHolder.AutoRemove = autoRemove;

            Add(sessionHolder);
            StartQueuedSessions();
        }

        /// <summary>
        /// Starts registered sessions in a managed way.
        /// </summary>
        public void StartQueuedSessions()
        {
            var running = Holders.Where(x => x.Session.Status == SessionStatus.Running);
            var waiting = Holders.Where(x => x.Session.Status == SessionStatus.NotStarted && x.Session.IsReady());

            if (waiting.Count() == 0 || running.Count() >= maxRunningSessions) return;

            int toBeStarted = Math.Min(maxRunningSessions - running.Count(), waiting.Count());
            for (int i = 0; i < toBeStarted; i++)
            {
                var sessionHolder = waiting.ElementAt(i);
                var task = sessionHolder.StartTask(false);
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
