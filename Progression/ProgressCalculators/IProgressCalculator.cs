using System.Diagnostics;

namespace Progression.ProgressCalculators
{
    public interface IProgressCalculator
    {
        /// <summary> Calculates the progress of this task. </summary>
        /// <param name="childProgress">The progress of the current step</param>
        float CalculateProgress(float childProgress);
        /// <summary> Advances to the next step. </summary>
        void NextStep();
    }
}
