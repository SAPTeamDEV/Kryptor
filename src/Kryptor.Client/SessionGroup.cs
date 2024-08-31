using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Collections.Specialized.BitVector32;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides High-Performance session monitoring solution.
    /// </summary>
    public partial class SessionGroup : ICollection<SessionHolder>
    {
        object _lockStatus = new object();
        object _lockStart = new object();
        object _lockEnd = new object();

        int _slotId = 0;
        private double progress = 0;

        protected double[] ProgressArray {  get; private set; }

        public List<SessionHolder> WaitingSessions { get; } = new List<SessionHolder>();

        public int Waiting { get; protected set; }

        public int Running { get; protected set; }

        public int Ended { get; protected set; }

        public int Completed { get; protected set; }

        public int Cancelled { get; protected set; }

        public int Failed { get; protected set; }

        public int Skipped { get; protected set; }

        public int Unknown { get; protected set; }

        public SessionStatus Status { get; protected set; }

        public SessionEndReason EndReason { get; protected set; }

        public List<string> Messages { get; } = new List<string>();

        public Stopwatch Timer { get; } = new Stopwatch();

        public double Progress => progress;

        protected void AddHooks(SessionHolder sessionHolder)
        {
            var session = sessionHolder.Session;

            var slotId = _slotId++;
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

        void OnSessionStarted(object sender, SessionEventArgs e)
        {
            lock (_lockStart)
            {
                if (Status != SessionStatus.NotStarted) return;

                Status = SessionStatus.Running;
                Timer.Start();

                IsLocked = true;
                ProgressArray = new double[Count];
            }
        }

        void OnSessionProgressChanged(int slotId, object sender, SessionUpdateEventArgs e)
        {
            if (e.Progress <= 0 ||  e.Progress > 100) return;

            var fragment = e.Progress / Count;
            Interlocked.Exchange(ref progress, progress + fragment - ProgressArray[slotId]);
            ProgressArray[slotId] = fragment;
        }

        void OnSessionEnded(object sender, SessionEventArgs e)
        {
            lock (_lockEnd)
            {
                if (e.EndReason == SessionEndReason.Completed) Completed++;
                else if (e.EndReason == SessionEndReason.Failed) Failed++;
                else if (e.EndReason == SessionEndReason.Cancelled) Cancelled++;
                else if (e.EndReason == SessionEndReason.Skipped) Skipped++;
                else Unknown++;

                ISession session = (ISession)sender;
                Array.ForEach(e.Messages, (x) => Messages.Add($"{session.Name} -> {x}"));

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
