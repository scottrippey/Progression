using System;
using System.Diagnostics;
using System.Threading;

namespace Progression.ProgressCalculators
{
    [DebuggerStepThrough]
    public class ProgressCalcUnknownTimer : ProgressCalcUnknown, IDisposable
    {
        /// <summary>
        /// Represents an enhanced version of ProgressUnknown.
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
        /// <param name="parent"></param>
        public ProgressCalcUnknownTimer(float estimatedDuration, float estimatedWeight, int interval, Progress parent)
            : base(estimatedDuration * (1000f / interval), estimatedWeight)
        {
            this.Interval = interval;
            // Setup the timer:
            this.timer = new Timer(timerCallback, null, interval, interval);
            this.parent = parent;
        }
        private Timer timer;
        private Progress parent;

        /// <summary> The duration, in milliseconds, between Timer events
        /// </summary>
        public int Interval { get; private set; }

        /// <summary> This is the Timer callback:
        /// </summary>
        private void timerCallback(object state)
        {
            if (this.timer == null) return; // We are disposed, so ignore queued timer events!
            this.NextStep();
            this.parent.OnProgressChanged();
            //this.parent.NextStep();
        }

        public void Dispose()
        {
            // When we are finished, let's dispose the timer,
            // and set it to null to indicate that we're disposed.
            if (this.timer != null) timer.Dispose();
            this.timer = null;

            this.parent = null;
        }

    }
}
