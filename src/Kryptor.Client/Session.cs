
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public List<ISession> SessionDependencies { get; }

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
            SessionDependencies = new List<ISession>();
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Status = SessionStatus.Running;
            Timer.Start();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsReady())
                {
                    throw new InvalidOperationException("You may not start this session at the moment");
                }

                bool result = await RunAsync(cancellationToken);

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
                Description = $"{ex.GetType().Name}: {ex.Message}";
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
        public virtual bool IsReady()
        {
            foreach (var session in SessionDependencies)
            {
                if (session.Status == SessionStatus.Ended)
                {
                    if (session.EndReason == SessionEndReason.Completed)
                    {
                        continue;
                    }

                    EndReason = SessionEndReason.Skipped;
                    Status = SessionStatus.Ended;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the session in a managed manner.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor the task.
        /// </param>
        /// <returns>
        /// true if the session ends successfully and false when the session ends with a handled error.
        /// </returns>
        protected abstract Task<bool> RunAsync(CancellationToken cancellationToken);
    }
}
