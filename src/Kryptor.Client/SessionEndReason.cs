using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents reason of session ending.
    /// </summary>
    public enum SessionEndReason
    {
        /// <summary>
        /// The session has not ended yet.
        /// </summary>
        None,

        /// <summary>
        /// The session ended successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The session failed with an exception.
        /// </summary>
        Failed,

        /// <summary>
        /// The session ended by session host cancellation request.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The session has been skipped.
        /// </summary>
        Skipped,
    }
}
