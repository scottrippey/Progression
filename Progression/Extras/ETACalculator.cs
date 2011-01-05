using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ProgressItem = System.Collections.Generic.KeyValuePair<long, float>;


namespace Progression.Extras
{
    /// <summary>
    /// Calculates the "Estimated Time of Arrival", 
    /// based on the rate of progress over time.
    /// </summary>
    public class ETACalculator 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumSize"></param>
        /// <param name="maximumSize"></param>
        public ETACalculator(int minimumSize, int maximumSize)
        {
            this.minimumSize = minimumSize;
            this.maximumSize = maximumSize;
            this.queue = new Queue<ProgressItem>(maximumSize);
            this.timer = Stopwatch.StartNew();
        }

        private int minimumSize;
        private int maximumSize;
        private readonly Stopwatch timer;
        private readonly Queue<ProgressItem> queue;

        private ProgressItem current;
        private ProgressItem oldest;

        public void Reset()
        {
            queue.Clear();

            timer.Reset();
            timer.Start();
        }

        /// <summary> Adds the current progress to the calculation of ETA.
        /// </summary>
        /// <param name="progress">The current level of completion.
        /// Must be between 0.0 and 1.0 (inclusively).</param>
        public void Add(float progress)
        {
            // Clear space for this item:
            while (queue.Count >= this.maximumSize)
            {
                this.oldest = queue.Dequeue();
            }

            // Queue this item:
            long currentTicks = timer.ElapsedTicks;
            this.current = new ProgressItem(currentTicks, progress);
            this.queue.Enqueue(this.current);

            if (this.queue.Count == 1)
            {
                this.oldest = this.current;
            }
        }

        /// <summary> Calculates the duration until the ETA
        /// </summary>
        public TimeSpan CompletedIn
        {
            get
            {
                // Create local copies of the oldest & current,
                // so that another thread can update them without locking:
                var oldest = this.oldest;
                var current = this.current;

                // Make sure we have enough items:
                if (queue.Count < this.minimumSize || oldest.Value == current.Value)
                {
                    return TimeSpan.MaxValue;
                }

                // Calculate the estimated finished time:
                double finishedInTicks = (1.0d - current.Value) * (current.Key - oldest.Key) / (current.Value - oldest.Value);

                return TimeSpan.FromSeconds(finishedInTicks / Stopwatch.Frequency);
            }
        }

        /// <summary> Calculates the ETA
        /// </summary>
        public DateTime CompletedAt
        {
            get
            {
                return DateTime.Now.Add(CompletedIn);
            }
        }
        
        /// <summary> Returns True when there is enough data to calculate the ETA.
        /// Returns False if the ETA is still calculating.
        /// </summary>
        public bool ETAIsAvailable
        {
            get
            {
                // Make sure we have enough items:
                if (queue.Count < this.minimumSize || oldest.Value == current.Value)
                {
                    return false;
                }
                return true;
            }
        }

    }

}
