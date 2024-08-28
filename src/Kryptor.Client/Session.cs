
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
        private double progress = 0;
        private string description = "";
        private SessionStatus status = SessionStatus.NotStarted;
        private SessionEndReason endReason;
        private Exception exception;
        private bool isPaused;

        /// <inheritdoc/>
        public virtual string Name => GetType().Name;

        /// <inheritdoc/>
        public virtual double Progress
        {
            get => progress;

            protected set
            {
                ThrowIfEnded();
                if (value == progress) return;

                progress = value;
                if (IsRunning) OnProgressChanged();
            }
        }

        /// <inheritdoc/>
        public virtual string Description
        {
            get => description;

            protected set
            {
                ThrowIfEnded();
                if (value == description) return;

                description = value;
                if (IsRunning) OnDescriptionChanged();
            }
        }

        /// <inheritdoc/>
        public bool IsRunning => Status == SessionStatus.Running || Status == SessionStatus.Managed;

        /// <inheritdoc/>
        public bool IsPaused
        {
            get => isPaused;

            protected set
            {
                ThrowIfNotStarted();
                ThrowIfEnded();
                if (isPaused == value) return;

                isPaused = value;

                if (value)
                {
                    Timer?.Stop();
                }
                else
                {
                    Timer?.Start();
                }

                OnPauseStatusChanged();
            }
        }

        /// <inheritdoc/>
        public virtual bool IsHidden => false;

        /// <inheritdoc/>
        public SessionStatus Status
        {
            get => status;

            protected set
            {
                ThrowIfEnded();
                if (value == status) return;
                if (value < status) throw new InvalidOperationException("Cannot revert the session's status");

                status = value;
                if (value == SessionStatus.Managed) return;

                OnStatusChanged();

                if (value == SessionStatus.Running)
                {
                    OnSessionStarted();
                }
                else if (value == SessionStatus.Ended)
                {
                    OnSessionEnded();
                }
            }
        }

        /// <inheritdoc/>
        public SessionEndReason EndReason
        {
            get => endReason;

            protected set
            {
                ThrowIfEnded();
                endReason = value;
            }
        }

        /// <inheritdoc/>
        public Exception Exception
        {
            get => exception;

            protected set
            {
                ThrowIfEnded();
                exception = value;
            }
        }

        /// <inheritdoc/>
        public Stopwatch Timer { get; }

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

        /// <inheritdoc/>
        public event EventHandler<SessionEventArgs> SessionStarted;

        /// <inheritdoc/>
        public event EventHandler<SessionUpdateEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public event EventHandler<SessionUpdateEventArgs> DescriptionChanged;

        /// <inheritdoc/>
        public event EventHandler<SessionEventArgs> StatusChanged;

        /// <inheritdoc/>
        public event EventHandler<SessionEventArgs> SessionEnded;

        /// <inheritdoc/>
        public event EventHandler<SessionUpdateEventArgs> SessionPaused;

        /// <inheritdoc/>
        public event EventHandler<SessionUpdateEventArgs> SessionResumed;

        /// <summary>
        /// Sets all session properties to thir default data.
        /// </summary>
        protected Session()
        {
            Timer = new Stopwatch();
            Messages = new List<string>();

            Dependencies = new List<ISession>();
            Dependents = new List<ISession>();
        }

        /// <inheritdoc/>
        public async Task StartAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            ThrowIfEnded();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (IsRunning)
                {
                    throw new InvalidOperationException("The session is already running");
                }

                if (!IsReady(cancellationToken))
                {
                    throw new InvalidOperationException("You may not start this session at the moment");
                }

                Status = SessionStatus.Running;
                Timer.Start();

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
                bool result = session.AddDependency(this);

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
            if (Status == SessionStatus.Ended) return false;

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

        /// <inheritdoc/>
        public virtual async Task<TResponse> SendRequest<TResponse>(ISessionHost sessionHost, SessionRequest<TResponse> request, CancellationToken cancellationToken)
        {
            IsPaused = true;

            TResponse response = await sessionHost.OnSessionRequest(this, request, cancellationToken);

            IsPaused = false;

            return response;
        }

        private SessionUpdateEventArgs CollectSessionUpdateData()
        {
            return new SessionUpdateEventArgs()
            {
                Progress = Progress,
                Description = Description,
                IsPause = IsPaused,
            };
        }

        private SessionEventArgs CollectSessionData()
        {
            return new SessionEventArgs()
            {
                Progress = Progress,
                Description = Description,
                IsPause = IsPaused,
                Status = Status,
                EndReason = EndReason,
                Exception = Exception,
                Messages = Messages.ToArray()
            };
        }

        /// <summary>
        /// Triggers the <see cref="SessionStarted"/> event.
        /// </summary>
        protected void OnSessionStarted() => SessionStarted?.Invoke(this, CollectSessionData());

        /// <summary>
        /// Triggers the <see cref="ProgressChanged"/> event.
        /// </summary>
        protected void OnProgressChanged() => ProgressChanged?.Invoke(this, CollectSessionUpdateData());

        /// <summary>
        /// Triggers the <see cref="DescriptionChanged"/> event.
        /// </summary>
        protected void OnDescriptionChanged() => DescriptionChanged?.Invoke(this, CollectSessionUpdateData());

        /// <summary>
        /// Triggers the <see cref="SessionPaused"/> or <see cref="SessionResumed"/> events.
        /// </summary>
        protected void OnPauseStatusChanged()
        {
            if (IsPaused)
            {
                SessionPaused?.Invoke(this, CollectSessionUpdateData());
            }
            else
            {
                SessionResumed?.Invoke(this, CollectSessionUpdateData());
            }
        }

        /// <summary>
        /// Triggers the <see cref="StatusChanged"/> event.
        /// </summary>
        protected void OnStatusChanged() => StatusChanged?.Invoke(this, CollectSessionData());

        /// <summary>
        /// Triggers the <see cref="SessionEnded"/> event.
        /// </summary>
        protected void OnSessionEnded() => SessionEnded?.Invoke(this, CollectSessionData());

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the session is not started.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected void ThrowIfNotStarted()
        {
            if (Status == SessionStatus.NotStarted) throw new InvalidOperationException("The session is not started yet");
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the session is ended.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected void ThrowIfEnded()
        {
            if (Status == SessionStatus.Ended) throw new InvalidOperationException("The session is ended");
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
