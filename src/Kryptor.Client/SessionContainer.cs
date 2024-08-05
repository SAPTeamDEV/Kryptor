using System;
using System.Collections.Generic;
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

        private void ResetCache()
        {
            sessions = null;
            tasks = null;
            tokenSources = null;
            holders = null;
        }
    }
}
