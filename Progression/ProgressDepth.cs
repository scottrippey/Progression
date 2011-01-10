using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Progression
{
    /// <summary> 
    /// An integer value that determines 
    /// the maximum number of nested progress tasks. 
    /// Progress reported at deeper levels will be ignored. 
    /// All negative values are equivalent to "Auto".
    /// </summary>
    public enum ProgressDepth : int 
    {
        /// <summary>
        /// Indicates that the maximum depth for this
        /// task should be automatic.
        /// This means that if this task falls within
        /// the maximum depth of any parent task,
        /// then this task's progress will be reported
        /// to the parent task.
        /// Otherwise, this task's progress will be ignored!
        /// </summary>
        Auto = -1,
        /// <summary>
        /// There is no limit on the maximum depth for this task.
        /// </summary>
        Unlimited = int.MaxValue,
    }
}
