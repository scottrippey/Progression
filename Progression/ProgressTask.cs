﻿using System;
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

        /// <summary> If polling isn't enabled, then progress calculation might be skipped. </summary>
        private bool pollingEnabled;
        /// <summary> If polling is enabled, this will hold the current progress info. </summary>
        private ProgressChangedInfo currentProgress;
        /// <summary> If polling is enabled, this will indicate whether the current progress has been accessed since last updated.
        /// If it hasn't been accessed yet, it will only be updated with higher-priority events.
        /// </summary>
        private bool currentProgressAccessed;

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

        #region: Properties :

        /// <summary>
        /// Retrieves the current task's progress.
        /// This is only available if polling is enabled.
        /// To enable polling, call <see cref="EnablePolling()" />.
        /// </summary>
        public ProgressChangedInfo CurrentProgress
        {
            get
            {
                currentProgressAccessed = true;
                return currentProgress;
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
        /// <summary> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="maxDepth"> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </param>
        public ProgressTask SetMaxDepth(int maxDepth)
        {
            this.maximumDepth = (ProgressDepth)maxDepth;
            return this;
        }

        /// <summary>
        /// Enables progress polling. 
        /// Use <see cref="CurrentProgress"/> to retrieve the task's current progress. 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        public ProgressTask EnablePolling()
        {
            EnablePolling(ProgressDepth.Unlimited);
            return this;
        }
        /// <summary>
        /// Enables progress polling. 
        /// Use <see cref="CurrentProgress"/> to retrieve the task's current progress. 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        public ProgressTask EnablePolling(ProgressDepth maximumDepth)
        {
            this.pollingEnabled = true;
            this.currentProgressAccessed = true;
            this.currentProgress = new ProgressChangedInfo(new ProgressInfo(0, null, null));
            this.maximumDepth = maximumDepth;
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

                // Raise events or update Polling:
                ProgressChangedInfo progressChangedInfo = null;
                // Raise the event if necessary:
                if (task.progressChanged != null)
                {
                    progressChangedInfo = new ProgressChangedInfo(allProgress);
                    task.progressChanged(progressChangedInfo);
                }
                // Update the CurrentProgress so that it can be used for polling.
                if (task.pollingEnabled)
                {
                    // If the current CurrentProgress hasn't been accessed,
                    // then we will only update if the new item is higher priority (lower depth):
                    if (task.currentProgressAccessed || depth < (ProgressDepth)task.currentProgress.Count)
                    {
                        progressChangedInfo = progressChangedInfo ?? new ProgressChangedInfo(allProgress);

                        task.currentProgressAccessed = false;
                        task.currentProgress = progressChangedInfo;
                    }
                }

                // Traverse the progress stack:
                depth++;
                task = task.parent;

                // Determine if we've reached the bottom of the stack or the maximum depth:
                if (task == null || task.maximumDepth < depth)
                {
                    break;
                }
            }

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
