using System.Diagnostics;

namespace Progression.ProgressTasks
{
    [DebuggerStepThrough]
    public class ProgressTaskProportional : ProgressTask
    {
        public ProgressTaskProportional(float[] stepProportions)
        {
            this.stepProportions = stepProportions;
            // Calculate related properties:
            this.Completed = 0f;
            var total = 0f;
            foreach (var p in stepProportions)
                total += p;
            this.Total = total;
        }

        /// <summary>Step Proportions is a list of the relative proportion of each step.</summary>
        protected readonly float[] stepProportions;
        /// <summary> Contains the sum of all step proportions </summary>
        public float Total { get; private set; }
        /// <summary> Contains the sum of all completed step proportions </summary>
        public float Completed { get; private set; }

        /// <summary> Advances the current progress task to the next step.
        /// Fires ProgressChanged events.
        /// </summary>
        public override void NextStep()
        {
            // Increment the Completed amount:
            if (this.stepIndex >= 0)
            {
                this.Completed += this.stepProportions[this.stepIndex];
            }
            base.NextStep();
        }

        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="stepProgress">The progress of nested steps</param>
        protected override float CalculateProgress(float stepProgress)
        {
            // Progressing through a list of items:
            float stepStart = this.Completed;
            float stepLength = this.stepProportions[this.stepIndex];
            float taskLength = this.Total;
            return (stepStart + stepProgress * stepLength) / taskLength;
        }

        public override string ToString()
        {
            return base.ToString(string.Format("Step {0} of {1} (Completed {2:0.0} of {3:0.0})", stepIndex + 1, stepProportions.Length, Completed, Total));
        }
    
    }

}
