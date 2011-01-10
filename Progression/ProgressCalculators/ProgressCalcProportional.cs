using System.Diagnostics;

namespace Progression.ProgressCalculators
{
    /// <summary> Represents a task with a specific number of steps.
    /// Progress is calculated proportionally for each step.
    /// </summary>
    [DebuggerStepThrough]
    public class ProgressCalcProportional : IProgressCalculator
    {
        public ProgressCalcProportional(float[] stepProportions)
        {
            this.StepIndex = -1;
            this.stepProportions = stepProportions;
            // Calculate related properties:
            this.Completed = 0f;
            var total = 0f;
            foreach (var p in stepProportions)
                total += p;
            this.Total = total;
        }

        /// <summary> The index of the current step. </summary>
        public int StepIndex { get; private set; }
        /// <summary>Step Proportions is a list of the relative proportion of each step.</summary>
        protected readonly float[] stepProportions;
        /// <summary> Contains the sum of all step proportions </summary>
        public float Total { get; private set; }
        /// <summary> Contains the sum of all completed step proportions </summary>
        public float Completed { get; private set; }

        /// <summary> Advances the current progress task to the next step. </summary>
        public void NextStep()
        {
            // Increment the Completed amount:
            if (this.StepIndex >= 0)
            {
                this.Completed += this.stepProportions[this.StepIndex];
            }
            // Increment the current index:
            this.StepIndex++;
        }

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        public float CalculateProgress(float stepProgress)
        {
            // Progressing through a list of items:
            float stepStart = this.Completed;
            float stepLength = this.stepProportions[this.StepIndex];
            float taskLength = this.Total;
            return (stepStart + stepProgress * stepLength) / taskLength;
        }

        public override string ToString()
        {
            return string.Format("Step {0} of {1} (Completed {2:0.0} of {3:0.0})", StepIndex + 1, stepProportions.Length, Completed, Total);
        }
    
    }

}
