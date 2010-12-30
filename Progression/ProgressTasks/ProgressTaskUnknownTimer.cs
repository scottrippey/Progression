using System.Diagnostics;
using System.Threading;

namespace Progression.ProgressTasks
{
    [DebuggerNonUserCode]
    public class ProgressTaskUnknownTimer : ProgressTaskUnknown
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
        public ProgressTaskUnknownTimer(float estimatedDuration, float estimatedWeight, int interval)
            : base(estimatedDuration * (1000f / interval), estimatedWeight)
        {
            this.Interval = interval;
            // Setup the timer:
            timer = new Timer(timerCallback, null, interval, interval);
        }
        private Timer timer;

        /// <summary> The duration, in milliseconds, between Timer events
        /// </summary>
        public int Interval { get; private set; }

        /// <summary> This is the Timer callback:
        /// </summary>
        private void timerCallback(object state)
        {
            if (timer == null) return; // We are disposed, so ignore queued timer events!
            base.NextStep();
        }

        public override void Dispose()
        {
            // When we are finished, let's dispose the timer,
            // and set it to null to indicate that we're disposed.
            timer.Dispose();
            timer = null;

            base.Dispose();
        }

    }
}
