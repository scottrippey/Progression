using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Progression.ProgressTasks
{
    //[DebuggerStepThrough]
    public abstract class ProgressTask : IDisposable
    {
        #region: ThreadStatic Stack :

        [ThreadStatic] private static ProgressTask currentTask;
        /// <summary> Returns the top of the current thread's progress stack
        /// </summary>
        public static ProgressTask CurrentTask { get { return currentTask; } }

        #endregion

        #region: Constructor :

        /// <summary> Pushes this Task to the top of the stack
        /// </summary>
        protected ProgressTask()
        {
            this.stepIndex = -1; // Always start at -1

            // Push this new task on the top of the task stack:
            this.parent = currentTask;
            if (currentTask != null) currentTask.child = this;
            currentTask = this;
            this.maximumCallbackDepth = (parent != null) ? parent.maximumCallbackDepth - 1 : -1;
        }

        #endregion

        #region: IDisposable / EndTask :

        /// <summary> Ends this task unsuccessfully, 
        /// meaning that the 100% complete event will not fire.
        /// </summary>
        public void Dispose()
        {
            EndTask(false);
        }
        /// <summary> Ends this task successfully,
        /// meaning that the 100% complete event will fire.
        /// This method should only be called once, and should only be called before the task is disposed.
        /// </summary>
        public void EndTask()
        {
            EndTask(true);
        }

        /// <summary> Ends this task, firing the 100% complete event if necessary, and pops the Progress stack.
        /// </summary>
        /// <param name="completedSuccessfully"> 
        /// Determines if the 100% complete event should be fired or skipped.
        /// </param>
        protected virtual void EndTask(bool completedSuccessfully)
        {
            // Only dispose once:
            if (this.isEnded) return;
            this.isEnded = true;

            // Make sure we are currently at the "top" of the stack:
            if (currentTask != this) throw new InvalidOperationException("There is a different Progress Task still open!  To avoid this issue, place all \"Progress.BeginTask()\" calls in a \"using\" block, or be sure to call \"Progress.EndTask()\".");

            // Report the 100% progress:
            if (completedSuccessfully)
            {
                this.OnProgressChanged(1.0f);
            }

            // Pop the stack:
            currentTask = this.parent;
            if (currentTask != null) currentTask.child = null;

            // Clear handlers:
            this.ProgressChanged = null;
        }

        #endregion

        #region: Fields :
		
        /// <summary>The parent field is used to implement a self-maintained stack.</summary>
        private ProgressTask parent;
        /// <summary>The child field is used to traverse the progress stack when calculating progress for a specific task (polling).</summary>
        private ProgressTask child;
        /// <summary> The maximum depth that this task's callback will listen </summary>
        private int maximumCallbackDepth;
        ///// <summary>Stores whether any ancestor has a callback, and is used to optimize progress calculation events.</summary>
        //private bool parentHasCallback;

        /// <summary> The name of the task </summary>
        protected string taskKey;
        /// <summary> Provides additional info about the task being performed </summary>
        protected object taskArg;

        /// <summary> The index of the currently executing step </summary>
        protected int stepIndex;

        /// <summary> Indicates that this task has ended and is disposed </summary>
        protected bool isEnded = false;
        
        #endregion

        #region: ToString :

        public override string ToString()
        {
            return ToString("Step " + (stepIndex + 1));
        }
        protected string ToString(string stepName)
        {
            if (taskKey != null)
            {
                if (taskArg != null)
                {
                    return string.Format("{0} - \"{1}\" ({2})", stepName, taskKey, taskArg);
                }
                return string.Format("{0} - \"{1}\"", stepName, taskKey);
            }
            return string.Format("{0}", stepName);
        }

        #endregion

        #region: Update Methods :

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
            Update(callback, int.MaxValue);
        }
        /// <summary> Attaches a ProgressChanged callback to the current task.
        /// This is usually done at the beginning of the task.
        /// </summary>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public void Update(ProgressChangedHandler callback, int maximumDepth)
        {
            this.ProgressChanged += callback;
            this.maximumCallbackDepth = maximumDepth;
        }

        #endregion

        #region: NextStep Methods :

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
        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        public virtual void NextStep()
        {
            // Advance the current step:
            this.stepIndex++;

            // Fire the ProgressChanged event:
            var myProgress = CalculateProgress(0.0f);
            OnProgressChanged(myProgress);
        }
        
        #endregion

        #region: Progress Calculation :

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        protected abstract float CalculateProgress(float stepProgress);
        
        private event ProgressChangedHandler ProgressChanged;
        /// <summary> Fires ProgressChanged events for the current task and all parent tasks
        /// </summary>
        protected void OnProgressChanged(float myProgress)
        {
            // Fire the ProgressChanged event for this and all parent items:

            var taskProgress = myProgress;
            var depth = 0;
            var allProgress = new Stack<ProgressInfo>();

            var task = this;
            while (true)
            {
                if (task.maximumCallbackDepth < depth)
                {
                    break;
                }

                allProgress.Push(new ProgressInfo(taskProgress, task.taskKey, task.taskArg));

                // Raise the event if necessary:
                if (task.ProgressChanged != null)
                {
                    var progressArgs = new ProgressChangedInfo(allProgress);
                    task.ProgressChanged(progressArgs);
                }

                // Traverse the progress stack:
                depth++;
                task = task.parent;

                // Determine if we've reached the bottom of the stack:
                if (task == null)
                {
                    break;
                }

                // Calculate the parent task's progress:
                taskProgress = task.CalculateProgress(taskProgress);
            }

        }

        /// <summary> Calculates the progress of the specified task.
        /// This can be used to "poll" a task's progress.
        /// This method is thread-safe.
        /// </summary>
        public ProgressChangedInfo CalculateProgress()
        {
            return CalculateProgress(int.MaxValue);
        }
        /// <summary> Calculates the progress of the specified task.
        /// This can be used to "poll" a task's progress.
        /// This method is thread-safe.
        /// </summary>
        /// <param name="maximumDepth">
        /// The maximum depth that will be used in the calculation.
        /// A value of 0 indicates that only this task will be used.
        /// Default is int.MaxValue.
        /// </param>
        public ProgressChangedInfo CalculateProgress(int maximumDepth)
        {
            if (maximumDepth <= 0) throw new ArgumentException("maximumDepth must be at least 1", "maximumDepth");

            // Collect all tasks stacked on this baseTask:
            // (this part needs to be thread safe)
            var stack = new Stack<ProgressTask>();
            var task = this;
            while (task != null && maximumDepth >= 0)
            {
                stack.Push(task);
                maximumDepth--;
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

            return new ProgressChangedInfo(allProgress);
        }

        #endregion

    }
}
