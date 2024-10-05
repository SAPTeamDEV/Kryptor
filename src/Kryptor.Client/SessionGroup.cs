using System.Diagnostics;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides High-Performance session monitoring solution.
    /// </summary>
    public partial class SessionGroup : ICollection<SessionHolder>
    {
        private readonly object _lockStatus = new object();
        private readonly object _lockStart = new object();
        private readonly object _lockEnd = new object();
        private int _slotId = 0;
        private double _progress = 0;
        private double[] _progressArray;
        private readonly List<string> messages = new List<string>();

        /// <summary>
        /// Gets the list of all sessions with <see cref="SessionStatus.NotStarted"/> status.
        /// </summary>
        /// <remarks>
        /// The main use of this property is to speed up the SessionManager's task schedular performance.
        /// </remarks>
        internal List<SessionHolder> WaitingSessions { get; } = new List<SessionHolder>();

        /// <summary>
        /// Gets the number of the sessions with <see cref="SessionStatus.NotStarted"/> status.
        /// </summary>
        public int Waiting { get; private set; }

        /// <summary>
        /// Gets the number of the sessions with <see cref="SessionStatus.Running"/> status.
        /// </summary>
        public int Running { get; private set; }

        /// <summary>
        /// Gets the number of the sessions with <see cref="SessionStatus.Ended"/> status.
        /// </summary>
        public int Ended { get; private set; }

        /// <summary>
        /// Gets the number of the ended sessions with <see cref="SessionEndReason.Completed"/> end reason.
        /// </summary>
        public int Completed { get; private set; }

        /// <summary>
        /// Gets the number of the ended sessions with <see cref="SessionEndReason.Canceled"/> end reason.
        /// </summary>
        public int Canceled { get; private set; }

        /// <summary>
        /// Gets the number of the ended sessions with <see cref="SessionEndReason.Failed"/> end reason.
        /// </summary>
        public int Failed { get; private set; }

        /// <summary>
        /// Gets the number of the ended sessions with <see cref="SessionEndReason.Skipped"/> end reason.
        /// </summary>
        public int Skipped { get; private set; }

        /// <summary>
        /// Gets the number of the ended sessions with <see cref="SessionEndReason.None"/> end reason.
        /// </summary>
        public int Unknown { get; private set; }

        /// <summary>
        /// Gets the brief report of all session's statuses.
        /// </summary>
        /// <remarks>
        /// When there are no sessions started yet, the status would be <see cref="SessionStatus.NotStarted"/>.
        /// <para>
        /// Whenever the first session is started, the status would be <see cref="SessionStatus.Running"/>.
        /// </para>
        /// When all of the sessions is ended, the status would be <see cref="SessionStatus.Ended"/>.
        /// </remarks>
        public SessionStatus Status { get; private set; }

        /// <summary>
        /// Gets a brief report of all session's end reasons.
        /// </summary>
        /// <remarks>
        /// When all sessions ended successfully, the end reason would be <see cref="SessionEndReason.Completed"/>.
        /// <para>
        /// When there is any cancelled session, the end reason would be <see cref="SessionEndReason.Canceled"/>.
        /// </para>
        /// <para>
        /// When there is any failed session, the end reason would be <see cref="SessionEndReason.Failed"/>.
        /// </para>
        /// <para>
        /// When there is any skipped session, the end reason would be <see cref="SessionEndReason.Skipped"/>.
        /// </para>
        /// <para>
        /// When there is any wierd session or when the session group is not started or still running with no ended session, the end reason would be <see cref="SessionEndReason.None"/>.
        /// </para>
        /// Except for the first case, in other cases the first occurrence is considered. even when there are still running or not started sessions.
        /// </remarks>
        public SessionEndReason EndReason { get; private set; }

        /// <summary>
        /// Gets all session messages.
        /// </summary>
        /// <remarks>
        /// This list only updated whenever a session is ended.
        /// </remarks>
        public string[] Messages => messages.ToArray();

        /// <summary>
        /// Gets the timer of the session group.
        /// </summary>
        /// <remarks>
        /// This timer started when the first session is started and stopped when the last session is ended.
        /// </remarks>
        public Stopwatch Timer { get; } = new Stopwatch();

        /// <summary>
        /// Gets the average progress percentage of all sessions.
        /// </summary>
        public double Progress => _progress;

        /// <summary>
        /// Wires up hooks to listen for session events.
        /// </summary>
        /// <param name="sessionHolder"></param>
        protected void AddHooks(SessionHolder sessionHolder)
        {
            ISession session = sessionHolder.Session;

            int slotId = _slotId++;
            WaitingSessions.Add(sessionHolder);
            Waiting++;

            session.StatusChanged += (o, e) => OnSessionStatusChanged(sessionHolder, o, e);
            session.SessionStarted += OnSessionStarted;
            session.ProgressChanged += (o, e) => OnSessionProgressChanged(slotId, o, e);
            session.SessionEnded += OnSessionEnded;
        }

        private void OnSessionStatusChanged(SessionHolder sessionHolder, object sender, SessionEventArgs e)
        {
            lock (_lockStatus)
            {
                if (e.PreviousStatus == SessionStatus.NotStarted)
                {
                    WaitingSessions.Remove(sessionHolder);
                }

                if (e.Status == SessionStatus.Running)
                {
                    Waiting--;
                    Running++;
                }
                else if (e.Status == SessionStatus.Ended && e.PreviousStatus == SessionStatus.Running)
                {
                    Running--;
                    Ended++;
                }
                else if (e.Status == SessionStatus.Ended && e.PreviousStatus == SessionStatus.NotStarted)
                {
                    Waiting--;
                    Ended++;
                }
            }
        }

        private void OnSessionStarted(object sender, SessionEventArgs e)
        {
            lock (_lockStart)
            {
                if (Status != SessionStatus.NotStarted) return;

                Status = SessionStatus.Running;
                Timer.Start();

                IsLocked = true;
                _progressArray = new double[Count];
            }
        }

        private void OnSessionProgressChanged(int slotId, object sender, SessionUpdateEventArgs e)
        {
            if (e.Progress <= 0 || e.Progress > 100) return;

            double fragment = e.Progress / Count;
            Interlocked.Exchange(ref _progress, _progress + fragment - _progressArray[slotId]);
            _progressArray[slotId] = fragment;
        }

        private void OnSessionEnded(object sender, SessionEventArgs e)
        {
            lock (_lockEnd)
            {
                if (e.EndReason == SessionEndReason.Completed) Completed++;
                else if (e.EndReason == SessionEndReason.Failed) Failed++;
                else if (e.EndReason == SessionEndReason.Canceled) Canceled++;
                else if (e.EndReason == SessionEndReason.Skipped) Skipped++;
                else Unknown++;

                ISession session = (ISession)sender;
                Array.ForEach(e.Messages, (x) => messages.Add($"{session.Name} -> {x}"));

                if (EndReason == SessionEndReason.None && e.EndReason != SessionEndReason.Completed)
                {
                    EndReason = e.EndReason;
                }

                if (Waiting == 0 && Running == 0)
                {
                    if (EndReason == SessionEndReason.None)
                    {
                        EndReason = SessionEndReason.Completed;
                    }

                    Status = SessionStatus.Ended;
                }
            }
        }
    }
}
