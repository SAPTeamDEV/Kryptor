﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents class to store session and related informations.
    /// </summary>
    public class SessionHolder
    {
        /// <summary>
        /// Gets the session instance.
        /// </summary>
        public ISession Session { get; }

        /// <summary>
        /// Gets the task of this session.
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// Gets the cancellation token source that controls the running session.
        /// </summary>
        public CancellationTokenSource TokenSource { get; set; }

        internal bool AutoRemove { get; set; }
        internal int Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionHolder"/> class.
        /// </summary>
        /// <param name="session">
        /// The session instance.
        /// </param>
        /// <param name="tokenSource">
        /// The cancellation token source that controls the running task.
        /// </param>
        public SessionHolder(ISession session, CancellationTokenSource tokenSource)
        {
            Session = session;
            TokenSource = tokenSource;
        }

        /// <summary>
        /// Starts the session task.
        /// </summary>
        /// <param name="throwIfRunning">
        /// Throw exception if the task already started.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public Task StartTask(bool throwIfRunning = true)
        {
            if (Task != null)
            {
                if (throwIfRunning)
                {
                    throw new InvalidOperationException("Session is already started.");
                }
                else
                {
                    return null;
                }
            }

            Task = Session.StartAsync(TokenSource.Token);
            return Task;
        }
    }
}