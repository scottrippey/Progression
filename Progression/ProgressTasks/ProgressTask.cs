using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Progression.ProgressTasks
{
    [DebuggerNonUserCode]
    public abstract class ProgressTask : IDisposable
    {
        [ThreadStatic] private static ProgressTask currentTask;
        public static ProgressTask CurrentTask { get { return currentTask; } }

        #region: Constructors / IDisposable :

        protected ProgressTask()
        {
            this.stepIndex = -1; // Always start at -1

            // Push this new task on the top of the task stack:
            this.parent = currentTask;
            if (currentTask != null) currentTask.child = this;
            currentTask = this;
            this.parentHasCallback = (parent != null && (parent.ProgressChanged != null || parent.parentHasCallback));
        }

        /// <summary> Disposes this task, firing the 100% complete event if necessary, and pops the Progress stack.
        /// </summary>
        public virtual void Dispose()
        {
            if (ProgressChanged != null)
            {
                // Report the 100% progress:
                ProgressChanged(new ProgressChangedInfo(new ProgressInfo(1.0f, this.taskKey, this.taskArg)));
            }

            // Make sure we are currently at the "top" of the stack:
            if (currentTask != this)
            {
                throw new InvalidOperationException("There is a Progress task still open!  To avoid this issue, place all \"Progress.BeginTask()\" calls in a \"using\" block, or be sure to call \"Progress.EndTask()\".");
            }

            // Pop the stack:
            currentTask = this.parent;
            if (currentTask != null) currentTask.child = null;

            // Clear handlers:
            ProgressChanged = null;
        }

        #endregion

        #region: Fields :
		
        /// <summary>The parent field is used to implement a self-maintained stack.</summary>
        private ProgressTask parent;
        /// <summary>The child field is used to traverse the progress stack when calculating progress for a specific task (polling).</summary>
        private ProgressTask child;
        /// <summary>Stores whether any ancestor has a callback, and is used to optimize progress calculation events.</summary>
        private bool parentHasCallback;

        /// <summary> The name of the task </summary>
        protected string taskKey;
        /// <summary> Provides additional info about the task being performed </summary>
        protected object taskArg;

        /// <summary>The index of the currently executing step.</summary>
        protected int stepIndex;
        
        #endregion

        #region: Public Methods :

        /// <summary> Changes the current task's TaskKey. </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public void Update(string newTaskKey)
        {
            this.taskKey = newTaskKey;
        }
        /// <summary> Changes the current task's TaskKey. </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="newTaskArg">Provides additional info about the task being performed</param>
        public void Update(string newTaskKey, object newTaskArg)
        {
            this.taskKey = newTaskKey;
            this.taskArg = newTaskArg;
        }
        /// <summary> Attaches a ProgressChanged callback to the current task.
        /// This is usually done at the beginning of the task.
        /// </summary>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public void Update(ProgressChangedHandler callback)
        {
            this.ProgressChanged += callback;
        }

        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public void NextStep(string newTaskKey)
        {
            this.taskKey = newTaskKey;
            NextStep();
        }
        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="newTaskArg">Provides additional info about the task being performed</param>
        public void NextStep(string newTaskKey, object newTaskArg)
        {
            this.taskKey = newTaskKey;
            this.taskArg = newTaskArg;
            NextStep();
        }

        #endregion

        #region: Virtual / Abstract Methods :

        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        public virtual void NextStep()
        {
            // Advance the current step:
            this.stepIndex++;

            // Fire the ProgressChanged event:
            OnProgressChanged();
        }

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        protected abstract float CalculateProgress(float stepProgress);

        #endregion

        #region: Progress Calculation :

        private event ProgressChangedHandler ProgressChanged;
        /// <summary> Fires ProgressChanged events for the current and all parents
        /// </summary>
        protected void OnProgressChanged()
        {
            if (this.stepIndex == 0)
            {
                // Fire the 0% event for ONLY this item (not parents)
                if (this.ProgressChanged != null)
                    this.ProgressChanged(new ProgressChangedInfo(new ProgressInfo(0f, this.taskKey, this.taskArg)));
                return;
            }

            // Fire the ProgressChanged event for this and all parent items:

            var taskProgress = 0.0f;
            var allProgress = new Stack<ProgressInfo>();

            var task = this;
            while (task != null)
            {
                // Determine the current task's progress:
                taskProgress = task.CalculateProgress(taskProgress);
                allProgress.Push(new ProgressInfo(taskProgress, task.taskKey, task.taskArg));

                // Raise the event if necessary:);
                if (task.ProgressChanged != null)
                {
                    var progressArgs = new ProgressChangedInfo(allProgress.ToArray());
                    task.ProgressChanged(progressArgs);
                }
                if (!task.parentHasCallback)
                {
                    break; // No more callbacks
                }

                task = task.parent;
            }

        }

        /// <summary> Calculates the progress of the specified task.
        /// This can be used to "poll" a task's progress.
        /// This method is thread-safe.
        /// </summary>
        public ProgressChangedInfo CalculateProgress()
        {
            // Collect all tasks stacked on this baseTask:
            // (this part needs to be thread safe)
            var stack = new Stack<ProgressTask>();
            var task = this;
            while (task != null)
            {
                stack.Push(task);
                task = task.child; // Thread-safe?
            }

            // Calculate the progress:
            var taskProgress = 0f;
            var allProgress = new Stack<ProgressInfo>(stack.Count);
            while (stack.Count > 0)
            {
                task = stack.Pop();

                // Determine the current task's progress:
                taskProgress = task.CalculateProgress(taskProgress);
                allProgress.Push(new ProgressInfo(taskProgress, task.taskKey, task.taskArg));
            }

            return new ProgressChangedInfo(allProgress.ToArray());
        }

        #endregion

    }
}
