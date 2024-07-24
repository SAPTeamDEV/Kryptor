using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents container to store sessions.
    /// </summary>
    public class SessionContainer
    {
        List<(ISession Session, Task Task, CancellationTokenSource TokenSource)> SessionPool = new List<(ISession Session, Task Task, CancellationTokenSource TokenSource)>();

        ISession[] sessions;
        Task[] tasks;
        CancellationTokenSource[] tokenSources;

        /// <summary>
        /// Gets an array of all sessions.
        /// </summary>
        public ISession[] Sessions
        {
            get
            {
                if (sessions == null)
                {
                    sessions = SessionPool.Select(x => x.Session).ToArray();
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
                    tasks = SessionPool.Select(x => x.Task).ToArray();
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
                    tokenSources = SessionPool.Select(x => x.TokenSource).ToArray();
                }

                return tokenSources;
            }
        }

        /// <summary>
        /// Adds new session to the container.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="task">
        /// The task started by session.
        /// </param>
        /// <param name="tokenSource">
        /// The token source that controls the cancellation token of the task.
        /// </param>
        public void Add(ISession session, Task task, CancellationTokenSource tokenSource)
        {
            SessionPool.Add((session, task, tokenSource));

            sessions = null;
            tasks = null;
            tokenSources = null;
        }
    }
}
