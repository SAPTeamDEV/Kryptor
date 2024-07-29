using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents interface to start and end program.
    /// </summary>
    public interface ISessionHost
    {
        /// <summary>
        /// Starts the host session in the current thread.
        /// </summary>
        void Start();

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
        Task NewSession(ISession session, bool autoRemove);
    }
}
