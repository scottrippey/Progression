using System.Diagnostics;

namespace Progression.ProgressTasks
{
    /// <summary>
    /// Represents a task with a fixed number of steps.
    /// </summary>
    [DebuggerNonUserCode]
    public class ProgressTaskFixed : ProgressTask
    {
        public ProgressTaskFixed(int stepCount)
        {
            this.StepCount = stepCount;
        }

        /// <summary>The number of steps in this task.
        /// </summary>
        public int StepCount { get; private set; }

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        protected override float CalculateProgress(float stepProgress)
        {
            // Progressing through a fixed number of items:
            return (this.stepIndex + stepProgress) / this.StepCount;
        }

    }

}
