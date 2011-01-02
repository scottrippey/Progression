using System.Diagnostics;

namespace Progression.ProgressTasks
{
    /// <summary>
    /// Represents a task with an unknown number of steps.
    /// As steps complete, progress gets closer to 100%,
    /// but never actually reaches it until the task completes.
    /// </summary>
    [DebuggerStepThrough]
    public class ProgressTaskUnknown : ProgressTask
    {
        /// <summary> </summary>
        /// <param name="estimatedSteps"></param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        public ProgressTaskUnknown(float estimatedSteps, float estimatedWeight)
        {
            // Progress Equation: W = S / (S + F)
            // Solved for F: F = S * (1 / W - 1)
            this.UnknownFactor = estimatedSteps * (1f / estimatedWeight - 1f);
        }

        /// <summary> Determines how "unknown" progress is calculated.  
        /// </summary>
        public float UnknownFactor { get; private set; }

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        protected override float CalculateProgress(float stepProgress)
        {
            // Progressing through the unknown:
            float stepStart = this.stepIndex + stepProgress;
            return stepStart / (stepStart + UnknownFactor);
        }

        public override string ToString()
        {
            return base.ToString(string.Format("Step {0} of Unknown", stepIndex + 1));
        }
    }
}
