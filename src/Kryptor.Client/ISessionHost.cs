using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents interface to start and end program.
    /// </summary>
    public interface ISessionHost
    {
        /// <summary>
        /// Gets the verbosity of the session host.
        /// </summary>
        bool Verbose { get; }

        /// <summary>
        /// Starts the host session in the current thread.
        /// </summary>
        /// <param name="context">
        /// The caller context.
        /// </param>
        void Start(ClientContext context);

        /// <summary>
        /// Ends the host session.
        /// </summary>
        /// <param name="cancelled">
        /// Whether the host session ending triggered by cancel request.
        /// </param>
        void End(bool cancelled);

        /// <summary>
        /// Starts a new session in this session host.
        /// </summary>
        /// <param name="session">
        /// A session with status <see cref="SessionStatus.NotStarted"/>.
        /// </param>
        /// <param name="autoRemove">
        /// Determines whether to automatically remove session after end.
        /// </param>
        void NewSession(ISession session, bool autoRemove);

        /// <summary>
        /// Adds given task to the task pool and be monitored by the session host.
        /// </summary>
        /// <param name="task">
        /// The task to be monitored.
        /// </param>
        void MonitorTask(Task task);
    }
}
