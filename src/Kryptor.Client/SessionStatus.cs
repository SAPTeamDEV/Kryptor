using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents status of the session.
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// The session has not started yet.
        /// </summary>
        NotStarted,

        /// <summary>
        /// The session is running and can be controlled by session host.
        /// </summary>
        Running,

        /// <summary>
        /// The session has ended naturally or by cancellation request.
        /// </summary>
        Ended,
    }
}
