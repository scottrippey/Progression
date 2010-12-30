using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Progression
{
    public delegate void ProgressChangedHandler(ProgressChangedInfo progressChangedInfo);

    [DebuggerNonUserCode]
    public class ProgressChangedInfo : IEnumerable<ProgressInfo>
    {
        public ProgressChangedInfo(params ProgressInfo[] allProgress)
        {
            this.allProgress = allProgress;
        }

        private ProgressInfo[] allProgress;

        /// <summary> Contains the total progress of the base task, which includes the progress of all child tasks. 
        /// This value is in the range of 0.0 to 1.0.
        /// </summary>
        public float TotalProgress { get { return allProgress[0].Progress; } }

        public ProgressInfo this[int index] { get { return allProgress[index]; } }
        public IEnumerator<ProgressInfo> GetEnumerator() { return (IEnumerator<ProgressInfo>) allProgress.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
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
        public float Progress { get; private set; }
        public string TaskKey { get; private set; }
        public object TaskArg { get; private set; }
    }
}
