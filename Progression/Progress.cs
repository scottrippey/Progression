using System;
using Progression.ProgressCalculators;

namespace Progression
{
    //[DebuggerStepThrough]
    public static class Progress 
    {
        #region: GetCurrentTask :

        /// <summary> Returns the top of the Progress stack.
        /// If the top is null, throws an appropriate exception.
        /// </summary>
        private static ProgressTask GetCurrentTask()
        {
            var currentTask = ProgressTask.CurrentTask;
            if (currentTask == null) throw new InvalidOperationException("No Progress task has been started");
            return currentTask;
        }

        #endregion

        #region: BeginTask Methods :

        /// <summary> Starts a task using a custom progress calculator. </summary>
        /// <param name="calculator">Any custom progress calculator</param>
        /// <returns>Returns an object that ends the task when it is disposed.</returns>
        public static ProgressTask BeginTask(IProgressCalculator calculator)
        {
            return new ProgressTask(calculator);
        }
        /// <summary> Starts a task with a specific number of steps. </summary>
        /// <param name="steps">The number of steps to be performed.</param>
        /// <returns>Returns an object that ends the task when it is disposed.</returns>
        public static ProgressTask BeginTaskFixed(int steps)
        {
            return new ProgressTask(new ProgressCalcFixed(steps));
        }
        /// <summary> Starts a task with a specific number of steps.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="stepProportions">The proportion of each step.
        /// For example, if you specify 4,6,10 then the steps will progress 20%,30%, and 50%.</param>
        /// <returns>Returns an object that ends the task when it is disposed.</returns>
        public static ProgressTask BeginTaskProportional(params float[] stepProportions)
        {
            return new ProgressTask(new ProgressCalcProportional(stepProportions));
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
        public static ProgressTask BeginTaskUnknown(int estimatedSteps, float estimatedWeight)
        {
            return new ProgressTask(new ProgressCalcUnknown(estimatedSteps, estimatedWeight));
        }

        #endregion

        #region: Chainable Update methods :

        /// <summary> Attaches the callback to fire when progress is reported.
        /// 
        /// This is usually called at the beginning of the task.
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maxDepth"> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </param>
        public static ProgressTask SetCallback(ProgressChangedHandler callback, ProgressDepth maxDepth)
        {
            return GetCurrentTask().SetCallback(callback, maxDepth);
        }
        /// <summary> Changes the current task's TaskKey. 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static ProgressTask SetTaskKey(string newTaskKey)
        {
            return GetCurrentTask().SetTaskKey(newTaskKey);
        }
        /// <summary> Changes the current task's TaskKey. 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="newTaskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="newTaskArg">Provides additional info about the task being performed</param>
        public static ProgressTask SetTaskKey(string newTaskKey, object newTaskArg)
        {
            return GetCurrentTask().SetTaskKey(newTaskKey, newTaskArg);
        }
        /// <summary> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". 
        /// Returns the current progress task, so that methods may be chained.
        /// </summary>
        /// <param name="maxDepth"> An integer value that determines the maximum number of nested progress tasks. Progress reported at deeper levels will be ignored. All negative values are equivalent to "Auto". </param>
        public static ProgressTask SetMaxDepth(ProgressDepth maxDepth)
        {
            return GetCurrentTask().SetMaxDepth(maxDepth);
        }

        #endregion

        #region: NextStep :

        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        public static void NextStep()
        {
            GetCurrentTask().NextStep();
        }
        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// 
        /// This is useful for ProgressCalculators that require custom NextStep behavior,
        /// such as the ProgressAmount calculator.
        /// </summary>
        public static void NextStep<TCalc>(Action<TCalc> nextStep) where TCalc : class, IProgressCalculator
        {
            GetCurrentTask().NextStep(nextStep);
        }

        #endregion

        #region: EndTask :

        /// <summary> Ends this task successfully,
        /// meaning that the 100% complete event will fire.
        /// This should be called before the task is disposed.
        /// </summary>
        public static void EndTask()
        {
            GetCurrentTask().EndTask();
        }
        
        #endregion

    }
}
