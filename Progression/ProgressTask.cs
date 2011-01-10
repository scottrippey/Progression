using System;
using System.Collections.Generic;
using System.Diagnostics;
using Progression.ProgressCalculators;

namespace Progression
{
    //[DebuggerStepThrough]
    public sealed class ProgressTask : IDisposable
    {
        #region: Internal ThreadStatic Stack :

        /// <summary> Holds the top of the current thread's progress stack </summary>
        [ThreadStatic] private static ProgressTask currentTask;
        public static ProgressTask CurrentTask { get { return currentTask; } }

        #endregion

        #region: Fields :
		
        /// <summary>The parent field is used to implement a self-maintained stack.</summary>
        private ProgressTask parent;
        /// <summary>The child field is used to traverse the progress stack when calculating progress for a specific task (polling).</summary>
        private ProgressTask child;

        /// <summary> Does the calculation. </summary>
        private IProgressCalculator calculator;

        /// <summary> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </summary>
        private ProgressDepth maximumDepth;
        /// <summary> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </summary>
        public ProgressDepth MaximumDepth { get { return maximumDepth; } }
        /// <summary> The callback that is fired when progress changes </summary>
        private event ProgressChangedHandler progressChanged;

        /// <summary> The name of the task </summary>
        private string taskKey;
        /// <summary> Provides additional info about the task being performed </summary>
        private object taskArg;

        /// <summary> Indicates that this task has ended and is disposed </summary>
        private bool isEnded = false;

        #endregion

        #region: Constructor :

        /// <summary> Pushes a new task to the top of the stack.
        /// </summary>
        /// <param name="calculator"></param>
        public ProgressTask(IProgressCalculator calculator)
        {
            this.calculator = calculator;

            // Push this new task on the top of the task stack:
            this.parent = currentTask;
            if (currentTask != null) currentTask.child = this;
            currentTask = this;

            // Calculate the new maximum depth:
            if (this.parent == null)
            {
                this.maximumDepth = ProgressDepth.Auto;
            }
            else
            {
                this.maximumDepth = parent.maximumDepth - 1;
            }
        }

        #endregion

        #region: Chainable Methods :

        /// <summary> Attaches the callback to fire when progress is reported.
        /// 
        /// This is usually called at the beginning of the task.
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public ProgressTask SetCallback(ProgressChangedHandler callback)
        {
            this.progressChanged += callback;
            this.maximumDepth = ProgressDepth.Unlimited;
            return this;
        }
        /// <summary> Attaches the callback to fire when progress is reported.
        /// 
        /// This is usually called at the beginning of the task.
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maxDepth"> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </param>
        public ProgressTask SetCallback(ProgressChangedHandler callback, ProgressDepth maxDepth)
        {
            this.progressChanged += callback;
            this.maximumDepth = maxDepth;
            return this;
        }

        /// <summary> Changes the current task's TaskKey. 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public ProgressTask SetTaskKey(string newTaskKey)
        {
            this.taskKey = newTaskKey;
            return this;
        }
        /// <summary> Changes the current task's TaskKey. 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="newTaskArg">Provides additional info about the task being performed</param>
        public ProgressTask SetTaskKey(string newTaskKey, object newTaskArg)
        {
            this.taskKey = newTaskKey;
            this.taskArg = newTaskArg;
            return this;
        }

        /// <summary> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="maxDepth"> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </param>
        public ProgressTask SetMaxDepth(ProgressDepth maxDepth)
        {
            this.maximumDepth = maxDepth;
            return this;
        }

        #endregion

        #region: NextStep :

        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        public void NextStep()
        {
            // Advance the current step:
            this.calculator.NextStep();

            // Fire the ProgressChanged event:
            this.OnProgressChanged();
        }
        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// 
        /// This is useful for ProgressCalculators that require custom NextStep behavior,
        /// such as the ProgressAmount calculator.
        /// </summary>
        public void NextStep<TCalc>(Action<TCalc> nextStep) where TCalc : class, IProgressCalculator
        {
            // Advance the current step:
            nextStep(this.calculator as TCalc);

            // Fire the ProgressChanged event:
            this.OnProgressChanged();
        }

        #endregion

        #region: Progress Calculation :

        /// <summary> Fires ProgressChanged events for the current task and all parent tasks
        /// </summary>
        private void OnProgressChanged()
        {
            // Fire the ProgressChanged event for this and all parent items:

            // (Skip if the current depth is too deep)
            if (this.maximumDepth <= ProgressDepth.Auto) return;

            var taskProgress = 0.0f;
            ProgressDepth depth = 0;
            var allProgress = new Stack<ProgressInfo>();

            var task = this;
            while (true)
            {
                // Calculate the task's progress:
                if (task.isEnded) // 100%!
                    taskProgress = 1.0f;
                else
                    taskProgress = task.calculator.CalculateProgress(taskProgress);

                allProgress.Push(new ProgressInfo(taskProgress, task.taskKey, task.taskArg));

                // Raise the event if necessary:
                if (task.progressChanged != null)
                {
                    var progressArgs = new ProgressChangedInfo(allProgress);
                    task.progressChanged(progressArgs);
                }

                // Traverse the progress stack:
                depth++;
                task = task.parent;

                // Determine if we've reached the bottom of the stack:
                if (task == null || task.maximumDepth < depth)
                {
                    break;
                }
            }

        }

        /// <summary> Calculates the progress of the specified task.
        /// This can be used to "poll" a task's progress.
        /// This method is thread-safe.
        /// </summary>
        public ProgressChangedInfo CalculateProgress()
        {
            var depth = this.maximumDepth;

            // Collect all tasks stacked on this baseTask:
            // (this part needs to be thread safe)
            var stack = new Stack<ProgressTask>((depth <= 0) ? 1 : (int)depth);
            var task = this;
            while (task != null && depth >= 0)
            {
                stack.Push(task);
                depth--;
                task = task.child; // Thread-safe?
            }

            // Calculate the progress:
            var taskProgress = 0f;
            var allProgress = new Stack<ProgressInfo>(stack.Count);
            while (stack.Count > 0)
            {
                task = stack.Pop();

                // Determine the current task's progress:
                taskProgress = task.calculator.CalculateProgress(taskProgress);
                allProgress.Push(new ProgressInfo(taskProgress, task.taskKey, task.taskArg));
            }

            return new ProgressChangedInfo(allProgress);
        }

        #endregion

        #region: Dispose / EndTask :

        /// <summary> Ends this task unsuccessfully, 
        /// meaning that the 100% complete event will not fire.
        /// Call Progress.EndTask() to successfully end this task with 100% progress.
        /// </summary>
        public void Dispose()
        {
            EndTask(false);
        }

        /// <summary> Ends this task successfully,
        /// meaning that the 100% complete event will fire.
        /// This should be called before the task is disposed.
        /// </summary>
        public void EndTask()
        {
            this.EndTask(true);
        }
        
        /// <summary> Ends this task, firing the 100% complete event if necessary, and pops the Progress stack.
        /// </summary>
        /// <param name="completedSuccessfully"> 
        /// Determines if the 100% complete event should be fired or skipped.
        /// </param>
        private void EndTask(bool completedSuccessfully)
        {
            // Only dispose once:
            if (this.isEnded) return;
            this.isEnded = true;

            // Tasks should always be disposed, so there 
            // shouldn't be any open "child" tasks,
            // but just in case, let's clean them up:
            if (this.child != null)
            {
                this.child.EndTask(false);
                this.child = null;
            }

            // Report the 100% progress:
            if (completedSuccessfully && this.maximumDepth > 0)
            {
                this.OnProgressChanged();
            }

            // Clear handlers:
            this.progressChanged = null;
            // Clear calculator:
            var calcDispose = this.calculator as IDisposable;
            if (calcDispose != null) calcDispose.Dispose();
            this.calculator = null;

            // Make sure "this" is on the top of the stack:
            if (currentTask == this)
            {
                // Pop the stack:
                currentTask = this.parent;
                if (currentTask != null) currentTask.child = null;
            } 
            else
            {
                // The only way this task wouldn't be on top of the stack
                // is if dispose is being called by a different thread.
                // Who would do such a thing?!?
                throw new InvalidOperationException("Progress disposed by a thread other than the one that created it.");
            }


        }

        #endregion

        #region: ToString :

        public override string ToString()
        {
            var stepName = (this.calculator == null) ? "(Progress Ignored)" : this.calculator.ToString();
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

    }
}
