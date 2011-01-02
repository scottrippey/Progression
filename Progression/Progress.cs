using System;
using System.Diagnostics;
using Progression.ProgressTasks;

namespace Progression
{
    [DebuggerStepThrough]
    public static class Progress 
    {
        #region: Static Interface :

        /// <summary> Starts a task with a specific number of steps. </summary>
        /// <param name="steps">The number of steps to be performed.</param>
        /// <returns>Returns an object that ends the task when it is disposed.</returns>
        public static ProgressTaskFixed BeginTask(int steps)
        {
            return new ProgressTaskFixed(steps);
        }
        /// <summary> Starts a task with a specific number of steps.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="stepProportions">The proportion of each step.
        /// For example, if you specify 4,6,10 then the steps will progress 20%,30%, and 50%.</param>
        /// <returns>Returns an object that ends the task when it is disposed.</returns>
        public static ProgressTaskProportional BeginTask(params float[] stepProportions)
        {
            return new ProgressTaskProportional(stepProportions);
        }
        /// <summary> Starts a task with an unknown number of steps.
        /// As tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="estimatedSteps">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <returns>Returns an object that ends the task when it is disposed.</returns>
        public static ProgressTaskUnknown BeginTaskUnknown(float estimatedSteps, float estimatedWeight)
        {
            return new ProgressTaskUnknown(estimatedSteps, estimatedWeight);
        }
        /// <summary> Starts an enhanced version of ProgressUnknown.
        /// Instead of waiting for sub-tasks to report progress,
        /// this task uses a timer to automatically report progress.
        /// As time ticks, progress gets closer to 100%, but never reaches it.
        /// 
        /// Note that ProgressChanged events will fire on a different thread!
        /// </summary>
        /// <param name="estimatedDuration">The estimated duration of this task, in seconds</param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimatedDuration.
        /// For example, if estimatedDuration is 100 and estimatedWeight is .75,
        /// then when 100 seconds have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <param name="interval">The rate at which to update the progress, in milliseconds</param>
        public static ProgressTaskUnknownTimer BeginTaskUnknown(float estimatedDuration, float estimatedWeight, int interval)
        {
            return new ProgressTaskUnknownTimer(estimatedDuration, estimatedWeight, interval);
        }

        private static ProgressTask CurrentTask
        {
            get
            {
                var currentTask = ProgressTask.CurrentTask;
                if (currentTask == null) throw new InvalidOperationException("No Progress task has been started");
                return currentTask;
            }
        }

        /// <summary> Ends and disposes the current task.
        /// Alternatively, you can put BeginTask in a "using" block
        /// which will automatically end the task.
        /// </summary>
        public static void EndTask()
        {
            CurrentTask.Dispose();
        }
        /// <summary> Ends and disposes the current task.
        /// Alternatively, you can put BeginTask in a "using" block
        /// which will automatically end the task.
        /// </summary>
        /// <param name="completedSuccessfully">
        /// Determines if the 100% complete event should be fired or skipped. Default is true.
        /// </param>
        public static void EndTask(bool completedSuccessfully)
        {
            CurrentTask.Dispose();
        }
        
        /// <summary> Changes the current task's TaskKey. </summary>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static void Update(string taskKey)
        {
            CurrentTask.Update(taskKey);
        }
        /// <summary> Changes the current task's TaskKey. </summary>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static void Update(string taskKey, object taskArg)
        {
            CurrentTask.Update(taskKey, taskArg);
        }
        /// <summary> Attaches a ProgressChanged callback to the current task.
        /// This is usually done at the beginning of the task.
        /// </summary>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public static void Update(ProgressChangedHandler callback)
        {
            CurrentTask.Update(callback);
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
        public static void Update(ProgressChangedHandler callback, int maximumDepth)
        {
            CurrentTask.Update(callback, maximumDepth);
        }

        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        public static void NextStep()
        {
            CurrentTask.NextStep();
        }
        /// <summary> Sets the current task key, and advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static void NextStep(string taskKey)
        {
            CurrentTask.NextStep(taskKey);
        }
        /// <summary> Sets the current task key, and advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static void NextStep(string taskKey, object taskArg)
        {
            CurrentTask.NextStep(taskKey, taskArg);
        }

    	#endregion
    }
}
