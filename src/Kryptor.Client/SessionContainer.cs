namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents container to store sessions.
    /// </summary>
    public partial class SessionContainer
    {
        private readonly object _addLock = new object();
        private readonly Dictionary<int, SessionHolder> SessionPool = new Dictionary<int, SessionHolder>();
        private readonly List<Task> TaskPool = new List<Task>();
        private ISession[] sessions;
        private ICollection<SessionHolder> holders;

        /// <summary>
        /// Gets an array of all sessions.
        /// </summary>
        public ISession[] Sessions
        {
            get
            {
                sessions ??= SessionPool.Values.Select(x => x.Session).ToArray();

                return sessions;
            }
        }

        /// <summary>
        /// Gets an array of all tasks.
        /// </summary>
        public Task[] Tasks => SessionPool.Values.Select(x => x.Task).Concat(TaskPool).Where(x => x != null).ToArray();

        /// <summary>
        /// Gets all session holders.
        /// </summary>
        public ICollection<SessionHolder> Holders
        {
            get
            {
                holders ??= SessionPool.Values;

                return holders;
            }
        }

        /// <summary>
        /// Adds new session to the container.
        /// </summary>
        /// <returns>The unique identifier of this entry.</returns>
        public int Add(SessionHolder sessionHolder)
        {
            lock (_addLock)
            {
                int rn = SessionPool.Keys.LastOrDefault() + 1;
                sessionHolder.Id = rn;
                SessionPool[rn] = sessionHolder;
                ResetCache();
                return rn;
            }
        }

        /// <summary>
        /// Adds just a task to <see cref="Tasks"/>.
        /// </summary>
        /// <param name="task">
        /// The task to be added.
        /// </param>
        public void AddMonitoringTask(Task task)
        {
            lock (_addLock)
            {
                TaskPool.Add(task);
                ResetCache();
            }
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
            holders = null;
        }
    }
}
