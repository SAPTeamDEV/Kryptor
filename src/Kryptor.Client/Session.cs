
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents abstraction to implement sessions.
    /// </summary>
    public abstract class Session : ISession
    {
        /// <inheritdoc/>
        public virtual double Progress { get; protected set; }

        /// <inheritdoc/>
        public virtual string Description { get; protected set; }

        /// <inheritdoc/>
        public bool IsRunning => Status == SessionStatus.Running || Status == SessionStatus.Managed;

        /// <inheritdoc/>
        public virtual bool IsHidden => false;

        /// <inheritdoc/>
        public SessionStatus Status { get; protected set; }

        /// <inheritdoc/>
        public SessionEndReason EndReason { get; protected set; }

        /// <inheritdoc/>
        public Exception Exception { get; protected set; }

        /// <inheritdoc/>
        public Stopwatch Timer { get; protected set; }

        /// <inheritdoc/>
        public List<string> Messages { get; }

        /// <summary>
        /// Gets the dependency list of the session. This session only starts when all of dependency sessions where completed successfully.
        /// </summary>
        protected List<ISession> Dependencies { get; }

        /// <summary>
        /// Gets the list of the dependents sessions. These sessions only starts when this session where completed successfully. Note this list is only intended for inter session communications and if you want to bind a session with this session you must call either <see cref="ContinueWith(ISession)"/> or add this session to the target session's <see cref="Dependencies"/> property.
        /// </summary>
        protected List<ISession> Dependents { get; }

        /// <summary>
        /// Sets all session properties to thir default data.
        /// </summary>
        protected Session()
        {
            Progress = 0;
            Description = "";

            Status = SessionStatus.NotStarted;
            EndReason = SessionEndReason.None;
            Exception = null;

            Timer = new Stopwatch();
            Messages = new List<string>();

            Dependencies = new List<ISession>();
            Dependents = new List<ISession>();
        }

        /// <inheritdoc/>
        public async Task StartAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            Status = SessionStatus.Running;
            Timer.Start();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsReady(cancellationToken))
                {
                    throw new InvalidOperationException("You may not start this session at the moment");
                }

                bool result = await RunAsync(sessionHost, cancellationToken);

                if (result)
                {
                    EndReason = SessionEndReason.Completed;
                }
            }
            catch (OperationCanceledException ocex)
            {
                EndReason = SessionEndReason.Cancelled;
                Exception = ocex;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aex && aex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                Description = $"{ex.GetType().Name}: {ex.Message}";
                Messages.Add(sessionHost.Verbose ? ex.ToString() : Description);

                EndReason = SessionEndReason.Failed;
                Exception = ex;
            }
            finally
            {
                Timer.Stop();
                Status = SessionStatus.Ended;
            }
        }

        /// <inheritdoc/>
        public virtual void ContinueWith(ISession session)
        {
            if (session.Status == SessionStatus.NotStarted)
            {
                var result = session.AddDependency(this);

                if (!result)
                {
                    throw new ApplicationException("Cannot add this session as dependency of " + session);
                }

                Dependents.Add(session);
            }
            else
            {
                throw new ArgumentException("The session is already started.");
            }
        }

        /// <inheritdoc/>
        public virtual bool AddDependency(ISession session)
        {
            if (Status != SessionStatus.NotStarted || Dependencies.Contains(session))
            {
                return false;
            }

            Dependencies.Add(session);
            return true;
        }

        /// <inheritdoc/>
        public virtual bool IsReady(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                SkipSession();
                return false;
            }

            foreach (ISession session in Dependencies)
            {
                if (session.Status == SessionStatus.Ended)
                {
                    if (session.EndReason == SessionEndReason.Completed)
                    {
                        continue;
                    }

                    SkipSession();
                }

                return false;
            }

            return true;
        }

        private void SkipSession()
        {
            EndReason = SessionEndReason.Skipped;
            Status = SessionStatus.Ended;
        }

        /// <summary>
        /// Starts the session in a managed manner.
        /// </summary>
        /// <param name="sessionHost">
        /// The parent session host.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <returns>
        /// true if the session ends successfully and false when the session ends with a handled error.
        /// </returns>
        protected abstract Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken);
    }
}
