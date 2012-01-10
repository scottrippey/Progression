using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Progression.Core
{
    public delegate void ProgressChangedHandler(ProgressChangedInfo progressChangedInfo);

    /// <summary>
    /// Represents the calculated progress of the progress stack.
    /// The first item (at index 0) is the base of the stack and contains the total progress.
    /// 
    /// This class is immutable, and therefore thread-safe.
    /// </summary>
    [DebuggerNonUserCode]
    public class ProgressChangedInfo : IList<ProgressInfo>
    {
        private readonly List<ProgressInfo> items;
        private readonly object currentStepArg;
        public ProgressChangedInfo(ProgressInfo progressInfo, object currentStepArg) 
        {
            items = new List<ProgressInfo>(1);
            items.Add(progressInfo);
            this.currentStepArg = currentStepArg;
        }
        public ProgressChangedInfo(IEnumerable<ProgressInfo> allProgress, object currentStepArg) 
        {
            items = new List<ProgressInfo>(allProgress);
            if (items.Count == 0) throw new ArgumentException("ProgressChangedInfo must contain at least 1 item!", "allProgress");
            this.currentStepArg = currentStepArg;
        }

        /// <summary> Contains the total progress of the base task, which includes the progress of all child tasks.
        /// This value is in the range of 0.0 to 1.0.
        /// </summary>
        public float TotalProgress { get { return this[0].Progress; } }

        /// <summary> The task on the bottom of the stack.  This is the task with the callback.
        /// </summary>
        public ProgressInfo BaseTask { get { return items[0]; } }

        /// <summary> The task on the top of the stack.  This is the task that caused the event.
        /// </summary>
        public ProgressInfo CurrentTask { get { return items[items.Count-1]; } }

        /// <summary> The current number of nested tasks
        /// </summary>
        public int CurrentDepth { get { return items.Count - 1; } }

        /// <summary>Provides additional info about the task being performed
        /// </summary>
        public object CurrentStepArg { get { return currentStepArg; } }

        /// <summary> Returns the ProgressInfo for the specified depth:
        /// </summary>
        /// <param name="depth"></param>
        public ProgressInfo this[int depth]
        {
            get { return items[depth]; }
        }

        #region: IList explicit implementation :
        // IList pass-through:
        ProgressInfo IList<ProgressInfo>.this[int index]
        {
            get { return items[index]; }
            set { throw new NotSupportedException(); }
        }
        IEnumerator<ProgressInfo> IEnumerable<ProgressInfo>.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        bool ICollection<ProgressInfo>.Contains(ProgressInfo item)
        {
            return items.Contains(item);
        }
        int IList<ProgressInfo>.IndexOf(ProgressInfo item)
        {
            return items.IndexOf(item);
        }
        void ICollection<ProgressInfo>.CopyTo(ProgressInfo[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        int ICollection<ProgressInfo>.Count
        {
            get { return items.Count; }
        }
        bool ICollection<ProgressInfo>.IsReadOnly
        {
            get { return true; }
        }
        // IList not supported:
        void ICollection<ProgressInfo>.Add(ProgressInfo item)
        {
            throw new NotSupportedException();
        }
        void ICollection<ProgressInfo>.Clear()
        {
            throw new NotSupportedException();
        }
        bool ICollection<ProgressInfo>.Remove(ProgressInfo item)
        {
            throw new NotSupportedException();
        }
        void IList<ProgressInfo>.Insert(int index, ProgressInfo item)
        {
            throw new NotSupportedException();
        }
        void IList<ProgressInfo>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        #endregion

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
        /// <summary> Identifies the task being performed.  
        /// </summary>
        public string TaskKey { get; private set; }
        /// <summary>Provides additional info about the task being performed
        /// </summary>
        public object TaskArg { get; private set; }
    }
}
