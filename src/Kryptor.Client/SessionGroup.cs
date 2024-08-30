using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides High-Performance session monitoring solution.
    /// </summary>
    public partial class SessionGroup : ICollection<ISession>
    {
        int _slotId = 0;

        protected double[] ProgressArray {  get; private set; }

        public int Waiting { get; protected set; }

        public int Running { get; protected set; }

        public int Ended { get; protected set; }

        public SessionStatus Status { get; protected set; }

        public SessionEndReason EndReason { get; protected set; }

        public Stopwatch Timer { get; }

        public double Progress => ProgressArray.DefaultIfEmpty(0).Average();

        protected void AddHooks(ISession session)
        {
            var slotId = _slotId++;
            Waiting++;

            session.StatusChanged += OnSessionStatusChanged;
            session.SessionStarted += OnSessionStarted;
            session.ProgressChanged += (o, e) => OnSessionProgressChanged(slotId, o, e);
            session.SessionEnded += OnSessionEnded;
        }

        private void OnSessionStatusChanged(object sender, SessionEventArgs e)
        {
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

        void OnSessionStarted(object sender, SessionEventArgs e)
        {
            if (Status != SessionStatus.NotStarted) return;

            Status = SessionStatus.Running;
            Timer.Start();

            IsLocked = true;
            ProgressArray = new double[Count];
        }

        void OnSessionProgressChanged(int slotId, object sender, SessionUpdateEventArgs e)
        {
            ProgressArray[slotId] = e.Progress;
        }

        void OnSessionEnded(object sender, SessionEventArgs e)
        {
            if (e.EndReason == SessionEndReason.Cancelled)
            {
                EndReason = e.EndReason;
            }

            if (Waiting == 0 && Running == 0)
            {
                Status = SessionStatus.Ended;
            }
        }
    }
}
