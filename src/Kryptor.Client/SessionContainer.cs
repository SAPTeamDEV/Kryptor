using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents container to store sessions.
    /// </summary>
    public partial class SessionContainer
    {
        private readonly Dictionary<int, SessionHolder> SessionPool = new Dictionary<int, SessionHolder>();
        private readonly List<Task> TaskPool = new List<Task>();
        private ISession[] sessions;
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
                return SessionPool.Values.Select(x => x.Task).Concat(TaskPool).Where(x => x != null).ToArray();
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
            _maxRunningSessions = maxRunningSessions;
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

        private void ResetCache()
        {
            sessions = null;
            tokenSources = null;
            holders = null;
        }
    }
}
