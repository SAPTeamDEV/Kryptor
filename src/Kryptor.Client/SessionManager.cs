using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Helpers;

namespace SAPTeam.Kryptor.Client
{
    public partial class SessionContainer
    {
        private readonly object _lock = new object();
        private readonly ISessionHost _sessionHost;
        private bool _cancellationRequested;
        private SessionGroup _sessionGroup;
        private readonly ConcurrentQueue<SessionHolder> _taskQueue = new ConcurrentQueue<SessionHolder>();
        private readonly List<Thread> _threads = new List<Thread>();

        /// <summary>
        /// Gets or sets the maximum allowed running sessions.
        /// </summary>
        public int MaxRunningSessions { get; set; }

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
            MaxRunningSessions = maxRunningSessions;

            // Initialize the thread pool
            for (int i = 0; i < maxRunningSessions; i++)
            {
                Thread thread = new Thread(ProcessTasks);
                thread.IsBackground = true;
                _threads.Add(thread);
                thread.Start();
            }
        }

        /// <summary>
        /// Waits until all sessions have been ended.
        /// </summary>
        /// <remarks>
        /// This method also waits for sessions that are not started yet.
        /// If the <paramref name="cancellationToken"/> was cancelled, this method will cancel all existing tokens in this container and waits for all of them to end.
        /// This method intended to be used by sub-session models. in standard session host based model, the monitoring, cancelling and ending is managed by various parts.
        /// But in sub-session models, you could achieve all of these features in this method.
        /// Using this method in session host is not recommended at all because this method is not intended for that porpose.
        /// </remarks>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public async Task WaitAll(CancellationToken cancellationToken)
        {
            while (!Sessions.All(x => x.Status == SessionStatus.Ended))
            {
                if (cancellationToken.IsCancellationRequested && !_cancellationRequested)
                {
                    Cancel();

                    _cancellationRequested = true;
                }

                await AsyncCompat.Delay(5);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Waits for all running tasks at the moment to end.
        /// </summary>
        /// <remarks>
        /// This method just waits for running tasks and does not care about waiting sessions (as they don't have a registered task).
        /// A Task must be registered by <see cref="AddMonitoringTask(Task)"/> that waits for not started sessions before calling this method.
        /// This method only intended to be used in <see cref="ISessionHost.End(bool)"/>.
        /// so using it in sub-session models is not recommended as it could be lead to unexpected session exit and losing control of sub-sessions.
        /// it's highly recommended to use <see cref="WaitAll(CancellationToken)"/> method in that situation.
        /// </remarks>
        public void WaitForRunningTasks() => Task.WaitAll(Tasks);

        /// <summary>
        /// Sends cancellation request to all cancellation tokens in this container.
        /// </summary>
        public void Cancel()
        {
            foreach (CancellationTokenSource token in TokenSources.Where(x => !x.IsCancellationRequested))
            {
                token.Cancel();
            }
        }

        /// <summary>
        /// Sets the provided <paramref name="sessionGroup"/> as the SessionManager's cache system.
        /// </summary>
        /// <remarks>
        /// The provided session group must be unused and unlocked. If a session group is already set, it must be fully finnished, otherwise it will throw a <see cref="InvalidOperationException"/>.
        /// A new instance of the <see cref="SessionGroup"/> class is always unlocked. It's highly recommended to use a new session group.
        /// </remarks>
        /// <param name="sessionGroup">
        /// A new session group with unlocked status.
        /// </param>
        /// <returns>
        /// <paramref name="sessionGroup"/>
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public SessionGroup SetSessionGroup(SessionGroup sessionGroup)
        {
            ValidateSessionGroup();

            if (_sessionGroup != null)
            {
                throw new InvalidOperationException("Cannot set new session group while the current one is running");
            }

            if (sessionGroup == null)
            {
                throw new ArgumentNullException(nameof(sessionGroup));
            }
            else if (sessionGroup.IsLocked)
            {
                throw new ArgumentException("Cannot use a locked session group");
            }

            _sessionGroup = sessionGroup;
            return sessionGroup;
        }

        private void ValidateSessionGroup()
        {
            if (_sessionGroup == null) return;

            if (_sessionGroup.Status == SessionStatus.Ended)
            {
                _sessionGroup = null;
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
        /// <param name="autoStart">
        /// Determines whether to automatically call the session manager's task schedular to start the session. This value is ignored when a valid session group was set.
        /// </param>
        public void NewSession(ISession session, bool autoRemove, bool autoStart)
        {
            ValidateSessionGroup();
            _sessionGroup?.ThrowIfLocked();

            SessionHolder sessionHolder = WrapSession(session, autoRemove);

            Add(sessionHolder);

            if (_sessionGroup != null)
            {
                _sessionGroup.Add(sessionHolder);
            }
            else if (autoStart)
            {
                StartQueuedSessions();
            }
        }

        /// <summary>
        /// Wraps the given <paramref name="session"/> into a <see cref="SessionHolder"/>.
        /// </summary>
        /// <param name="session">
        /// A session with status <see cref="SessionStatus.NotStarted"/>.
        /// </param>
        /// <param name="autoRemove">
        /// Determines whether to automatically remove session after end.
        /// </param>
        /// <returns></returns>
        public static SessionHolder WrapSession(ISession session, bool autoRemove)
        {
            if (session.Status != SessionStatus.NotStarted)
            {
                throw new ArgumentException("The session is already started.");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            SessionHolder sessionHolder = new SessionHolder(session, tokenSource)
            {
                AutoRemove = autoRemove
            };

            return sessionHolder;
        }

        /// <summary>
        /// Starts registered sessions in a managed way.
        /// </summary>
        /// <remarks>
        /// This is the core of the Session based Application Model logic.
        /// This task scheduler logic works with checking the number of running sessions (sessions with status <see cref="SessionStatus.Running"/>)
        /// and checking the waiting sessions (sessions with status <see cref="SessionStatus.NotStarted"/> and be ready).
        /// The ready to run sessions determined by calling the <see cref="ISession.IsReady(CancellationToken)"/> method, if the return value is <see langword="true"/> the session will be added to the waiting list, otherwise it will be ignored and rechecked at the next run of the task scheduler.
        /// There is some important notes about this process.
        /// <para>
        /// The task scheduler only runs after each <see cref="NewSession(ISession, bool, bool)"/>calls and session ends. so if there is two sessions and the second session does not get ready until first session end, the task scheduler won't try to run that session and it will stuck at waiting status.
        /// </para>
        /// <para>
        /// The <see cref="ISession.IsReady(CancellationToken)"/> should not throw ANY exceptions at all. it may break the scheduler.
        /// Although there is some layers to handle exceptions, but the <see cref="ISession.IsReady(CancellationToken)"/> is only allwed to response with <see langword="true"/> or <see langword="false"/>.
        /// </para>
        /// </remarks>
        public void StartQueuedSessions()
        {
            lock (_lock)
            {
                ValidateSessionGroup();

                QueueProcessImpl();
            }
        }

        private void QueueProcessImpl()
        {
            int runningCount;
            List<SessionHolder> waiting;

            if (_sessionGroup != null)
            {
                runningCount = _sessionGroup.Running;
                waiting = _sessionGroup.WaitingSessions;
            }
            else
            {
                runningCount = Holders.Count(x => x.Session.Status == SessionStatus.Running);
                waiting = Holders.Where(x => x.Session.Status == SessionStatus.NotStarted && SafeIsReady(x)).ToList();
            }

            if (waiting.Count == 0 || runningCount >= MaxRunningSessions) return;

            foreach (var sessionHolder in waiting)
            {
                _taskQueue.Enqueue(sessionHolder);
            }
        }

        private void ProcessTasks()
        {
            while (true)
            {
                if (_taskQueue.TryDequeue(out var sessionHolder))
                {
                    try
                    {
                        StartManagedSession(sessionHolder);
                    }
                    catch (Exception ex)
                    {
                        sessionHolder.Session.Messages.Add("Error in session start: " + ex.Message);
                    }
                }
                else
                {
                    Thread.Sleep(10); // Sleep for a short period to avoid busy-waiting
                }
            }
        }

        private static bool SafeIsReady(SessionHolder holder)
        {
            try
            {
                return holder.Session.IsReady(holder.TokenSource.Token);
            }
            catch
            {
                return false;
            }
        }

        private void StartManagedSession(SessionHolder sessionHolder)
        {
            Task task = sessionHolder.StartTask(_sessionHost, true);
            if (task != null)
            {
                task.ContinueWith(t =>
                {
                    StartQueuedSessions();
                });

                if (sessionHolder.AutoRemove)
                {
                    task.ContinueWith(t => Remove(sessionHolder.Id));
                }
            }
        }
    }
}
