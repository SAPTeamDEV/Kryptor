
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
        public double Progress { get; protected set; }

        /// <inheritdoc/>
        public string Description { get; protected set; }

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
        public List<ISession> Dependencies { get; }

        /// <inheritdoc/>
        public List<string> Messages { get; }

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
            Dependencies = new List<ISession>();
            Messages = new List<string>();
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
                Messages.Add(Description);

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
