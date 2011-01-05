using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Progression
{
    public delegate void ProgressChangedHandler(ProgressChangedInfo progressChangedInfo);

    /// <summary>
    /// Represents the calculated progress of the progress stack.
    /// The first item (at index 0) is the base of the stack and contains the total progress.
    /// </summary>
    [DebuggerNonUserCode]
    public class ProgressChangedInfo : List<ProgressInfo>
    {
        public ProgressChangedInfo(ProgressInfo progressInfo) : base(1)
        {
            Add(progressInfo);
        }
        public ProgressChangedInfo(IEnumerable<ProgressInfo> allProgress) : base(allProgress)
        {
            if (this.Count == 0) throw new ArgumentException("ProgressChangedInfo must contain at least 1 item!", "allProgress");
        }

        /// <summary> Contains the total progress of the base task, which includes the progress of all child tasks.
        /// This value is in the range of 0.0 to 1.0.
        /// </summary>
        public float TotalProgress { get { return this[0].Progress; } }

        /// <summary>
        /// The task on the bottom of the stack.  This is the task with the callback.
        /// </summary>
        public ProgressInfo BaseTask { get { return this[0]; } }

        /// <summary>
        /// The task on the top of the stack.  This is the task that caused the event.
        /// </summary>
        public ProgressInfo CurrentTask { get { return this[Count-1]; } }
    }

    [DebuggerNonUserCode]
    public class ProgressInfo
    {
        public ProgressInfo(float progress, string taskKey, object taskArg)
        {
            this.Progress = progress;
            this.TaskKey = taskKey;
            this.TaskArg = taskArg;
        }
        /// <summary> The total progress of this task, which incorporates the progress of sub-tasks.
        /// </summary>
        public float Progress { get; private set; }
        /// <summary> Identifies the task being performed.  Can be used for displaying progress.
        /// </summary>
        public string TaskKey { get; private set; }
        /// <summary>Provides additional info about the task being performed
        /// </summary>
        public object TaskArg { get; private set; }
    }
}
