using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Progression.Core;

namespace Progression.Demo
{
    public class ProgressWatcher : IDisposable
    {
        public ProgressWatcher()
        {
            this.Timer = new DispatcherTimer();
            this.Timer.Tick += this.Timer_Tick;
            this.Timer.Interval = TimeSpan.FromMilliseconds(100);

            this.MaximumDepth = 99;

            ProgressTask.ProgressStarting += this.Progress_Starting;
        }

        public DispatcherTimer Timer { get; private set; }

        public TimeSpan Interval { get { return Timer.Interval; } set { Timer.Interval = value; } }

        [DefaultValue("")]
        public string TaskKey { get; set; }

        [DefaultValue(99)]
        public int MaximumDepth { get; set; }

        public delegate void ProgressChangedEventHandler(object sender, ProgressChangedInfo progressInfo);
        public event ProgressChangedEventHandler ProgressChanged;

        private ProgressTask currentTask;
        private void Progress_Starting(ProgressTask progressTask)
        {
            // We're only looking for this specific task:
            if (this.currentTask != null || progressTask.TaskKey != this.TaskKey)
            {
                return;
            }

            // Keep track of this task so we can poll it:
            this.currentTask = progressTask;
            progressTask.EnablePolling(this.MaximumDepth);

            // Start polling for progress:
            Timer.Start();

            progressTask.SetCallbackEnded(this.OnProgressEnding);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Grab the current progress:
            var task = this.currentTask;
            if (task == null) // Let's be thread safe.
            {
                return;
            }

            var progress = task.CurrentProgress;

            // Raise the progress event:
            var progressChanged = this.ProgressChanged;
            if (progressChanged != null)
            {
                progressChanged(this, progress);
            }
        }


        private void OnProgressEnding(ProgressChangedInfo progresschangedinfo)
        {
            StopPolling();
        }
        private void StopPolling()
        {
            // Stop polling:
            var timer = this.Timer;
            if (timer != null)
            {
                Timer.Stop();
            }

            // Clear the task:
            this.currentTask = null;
        }

        public void Dispose()
        {
            StopPolling();

            // Clean up:
            ProgressTask.ProgressStarting -= this.Progress_Starting;
            this.Timer.Tick -= this.Timer_Tick;
            this.Timer = null;
        }
    }
}
