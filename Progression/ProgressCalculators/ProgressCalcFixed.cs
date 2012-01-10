using System.Diagnostics;

namespace Progression.ProgressCalculators
{
    /// <summary> Represents a task with a specific number of steps. 
    /// </summary>
    [DebuggerNonUserCode]
    public class ProgressCalcFixed : IProgressCalculator
    {
        public ProgressCalcFixed(int stepCount)
        {
            this.StepIndex = -1;
            this.StepCount = stepCount;
        }

        /// <summary> The index of the current step. </summary>
        public int StepIndex { get; private set; }

        /// <summary>The number of steps in this task. </summary>
        public int StepCount { get; private set; }

        /// <summary> Advances the current progress task to the next step. </summary>
        public void NextStep()
        {
            this.StepIndex++;
        }

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        public float CalculateProgress(float stepProgress)
        {
            // Progressing through a fixed number of items:
            return (this.StepIndex + stepProgress) / this.StepCount;
        }

        public override string ToString()
        {
            return string.Format("Step {0} of {1}", StepIndex + 1, StepCount);
        }
    }

}
