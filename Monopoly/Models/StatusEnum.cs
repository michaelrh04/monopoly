using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    /// <summary>
    /// Used for determining the status of processes (to be shown in the view).
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The process is pending; it has not begun.
        /// </summary>
        Pending,
        /// <summary>
        /// The process is ongoing.
        /// </summary>
        Ongoing,
        /// <summary>
        /// The process has been completed.
        /// </summary>
        Completed,
        /// <summary>
        /// The process has been terminated for a generic reason.
        /// </summary>
        Terminated,
        /// <summary>
        /// The process could not be completed.
        /// </summary>
        Failed,
        /// <summary>
        /// The process is available.
        /// </summary>
        Available,
        /// <summary>
        /// The process is unavailable.
        /// </summary>
        Unavailable
    }
}
