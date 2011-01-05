using System.Collections.Generic;
using System.Diagnostics;
using Progression.ProgressTasks;

namespace Progression
{
    [DebuggerNonUserCode]
    public static class ProgressExtensions
    {
        #region: Chainable methods :

        /// <summary> Chainable extension method that simply updates the current TaskKey.
        /// Same as calling Progress.Update(...).
        /// </summary>
        /// <param name="progress">The progress object simply gets passed-through, allowing chainability</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static TProgress UpdateTask<TProgress>(this TProgress progress, string taskKey) where TProgress : ProgressTask
        {
            Progress.Update(taskKey);
            return progress;
        }

        /// <summary> Chainable extension method that simply updates the current TaskKey and TaskArg.
        /// Same as calling Progress.Update(...).
        /// </summary>
        /// <param name="progress">The progress object simply gets passed-through, allowing chainability</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static TProgress UpdateTask<TProgress>(this TProgress progress, string taskKey, object taskArg) where TProgress : ProgressTask
        {
            Progress.Update(taskKey, taskArg);
            return progress;
        }

        /// <summary> Chainable extension method that simply attaches the callback.
        /// Same as calling Progress.UpdateCallback(...).
        /// </summary>
        /// <param name="progress">The progress object simply gets passed-through, allowing chainability</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public static TProgress UpdateTask<TProgress>(this TProgress progress, ProgressChangedHandler callback) where TProgress : ProgressTask
        {
            Progress.Update(callback);
            return progress;
        }
        /// <summary> Chainable extension method that simply attaches the callback.
        /// Same as calling Progress.UpdateCallback(...).
        /// </summary>
        /// <param name="progress">The progress object simply gets passed-through, allowing chainability</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static TProgress UpdateTask<TProgress>(this TProgress progress, ProgressChangedHandler callback, int maximumDepth) where TProgress : ProgressTask
        {
            Progress.Update(callback, maximumDepth);
            return progress;
        }

        #endregion

        #region: WithProgress methods :

        // Note: These methods wrap an IEnumerable, monitoring progress as the object is enumerated.

        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source">The Count property will be used to calculate progress as items are enumerated.</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this ICollection<T> source, string taskKey, object taskArg, ProgressChangedHandler callback, int maximumDepth)
        {
            return WithProgress(source, source.Count, taskKey, taskArg, callback, maximumDepth);
        }

        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source"></param>
        /// <param name="sourceCount">Used to calculate progress as items are enumerated. If the count is unknown, use the "WithProgressUnknown" overload.</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int sourceCount, string taskKey, object taskArg, ProgressChangedHandler callback, int maximumDepth)
        {
            using (Progress.BeginTask(sourceCount))
            {
                if (taskKey != null) Progress.Update(taskKey, taskArg);
                if (callback != null) Progress.Update(callback, maximumDepth);
                foreach (var item in source)
                {
                    Progress.NextStep();
                    yield return item;
                }
                Progress.EndTask();
            }
        }

        /// <summary> Tracks progress as the source is enumerated.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stepProportions">
        /// Determines the proportion of each step.
        /// For example, the filesize of a copy operation.
        /// </param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, float[] stepProportions, string taskKey, object taskArg, ProgressChangedHandler callback, int maximumDepth)
        {
            using (Progress.BeginTask(stepProportions))
            {
                if (taskKey != null) Progress.Update(taskKey, taskArg);
                if (callback != null) Progress.Update(callback, maximumDepth);
                foreach (var item in source)
                {
                    Progress.NextStep();
                    yield return item;
                }
                Progress.EndTask();
            }
        }

        /// <summary> Tracks progress as the source is enumerated.
        /// 
        /// Since the number of items is unknown,
        /// as tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="source">Note: If the source is a Collection, then the Count will be used and the estimatedSteps will be ignored.</param>
        /// <param name="estimatedCount">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgressUnknown<T>(this IEnumerable<T> source, float estimatedCount, float estimatedWeight, string taskKey, object taskArg, ProgressChangedHandler callback, int maximumDepth)
        {
            // Just in case the source is a Collection or List, we can use the Count so that the task isn't "Unknown":
            var sourceCollection = source as ICollection<T>;
            using ((sourceCollection != null) ? (ProgressTask)Progress.BeginTask(sourceCollection.Count) : Progress.BeginTaskUnknown(estimatedCount, estimatedWeight))
            {
                if (taskKey != null) Progress.Update(taskKey, taskArg);
                if (callback != null) Progress.Update(callback, maximumDepth);
                foreach (var item in source)
                {
                    Progress.NextStep();
                    yield return item;
                }
                Progress.EndTask();
            }
        }

        #endregion
    }

    /// <summary> This class contains overloads for the ProgressExtensions methods.
    /// It would be way easier to just have optional-parameters in the original methods,
    /// but this allows compatibility with .NET 3.5.
    /// </summary>
    [DebuggerNonUserCode]
    public static class ProgressExtensionsOverloads
    {
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source">The Count property will be used to calculate progress as items are enumerated.</param>
        public static IEnumerable<T> WithProgress<T>(this ICollection<T> source)
        {
            return ProgressExtensions.WithProgress(source, null, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source">The Count property will be used to calculate progress as items are enumerated.</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static IEnumerable<T> WithProgress<T>(this ICollection<T> source, string taskKey)
        {
            return ProgressExtensions.WithProgress(source, taskKey, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source">The Count property will be used to calculate progress as items are enumerated.</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static IEnumerable<T> WithProgress<T>(this ICollection<T> source, string taskKey, object taskArg)
        {
            return ProgressExtensions.WithProgress(source, taskKey, taskArg, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source">The Count property will be used to calculate progress as items are enumerated.</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public static IEnumerable<T> WithProgress<T>(this ICollection<T> source, ProgressChangedHandler callback)
        {
            return ProgressExtensions.WithProgress(source, null, null, callback, int.MaxValue);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source">The Count property will be used to calculate progress as items are enumerated.</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this ICollection<T> source, ProgressChangedHandler callback, int maximumDepth)
        {
            return ProgressExtensions.WithProgress(source, null, null, callback, maximumDepth);
        }

        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source"></param>
        /// <param name="sourceCount">Used to calculate progress as items are enumerated. If the count is unknown, use the "WithProgressUnknown" overload.</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int sourceCount)
        {
            return ProgressExtensions.WithProgress(source, sourceCount, null, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source"></param>
        /// <param name="sourceCount">Used to calculate progress as items are enumerated. If the count is unknown, use the "WithProgressUnknown" overload.</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int sourceCount, string taskKey)
        {
            return ProgressExtensions.WithProgress(source, sourceCount, taskKey, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source"></param>
        /// <param name="sourceCount">Used to calculate progress as items are enumerated. If the count is unknown, use the "WithProgressUnknown" overload.</param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int sourceCount, string taskKey, object taskArg)
        {
            return ProgressExtensions.WithProgress(source, sourceCount, taskKey, taskArg, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source"></param>
        /// <param name="sourceCount">Used to calculate progress as items are enumerated. If the count is unknown, use the "WithProgressUnknown" overload.</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int sourceCount, ProgressChangedHandler callback)
        {
            return ProgressExtensions.WithProgress(source, sourceCount, null, null, callback, int.MaxValue);
        }
        /// <summary> Tracks progress as the source is enumerated. </summary>
        /// <param name="source"></param>
        /// <param name="sourceCount">Used to calculate progress as items are enumerated. If the count is unknown, use the "WithProgressUnknown" overload.</param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int sourceCount, ProgressChangedHandler callback, int maximumDepth)
        {
            return ProgressExtensions.WithProgress(source, sourceCount, null, null, callback, maximumDepth);
        }

        /// <summary> Tracks progress as the source is enumerated.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stepProportions">
        /// Determines the proportion of each step.
        /// For example, the filesize of a copy operation.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, float[] stepProportions)
        {
            return ProgressExtensions.WithProgress(source, stepProportions, null, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stepProportions">
        /// Determines the proportion of each step.
        /// For example, the filesize of a copy operation.
        /// </param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, float[] stepProportions, string taskKey)
        {
            return ProgressExtensions.WithProgress(source, stepProportions, taskKey, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stepProportions">
        /// Determines the proportion of each step.
        /// For example, the filesize of a copy operation.
        /// </param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, float[] stepProportions, string taskKey, object taskArg)
        {
            return ProgressExtensions.WithProgress(source, stepProportions, taskKey, taskArg, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stepProportions">
        /// Determines the proportion of each step.
        /// For example, the filesize of a copy operation.
        /// </param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, float[] stepProportions, ProgressChangedHandler callback)
        {
            return ProgressExtensions.WithProgress(source, stepProportions, null, null, callback, int.MaxValue);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// Progress is calculated proportionally for each step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stepProportions">
        /// Determines the proportion of each step.
        /// For example, the filesize of a copy operation.
        /// </param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, float[] stepProportions, ProgressChangedHandler callback, int maximumDepth)
        {
            return ProgressExtensions.WithProgress(source, stepProportions, null, null, callback, maximumDepth);
        }

        /// <summary> Tracks progress as the source is enumerated.
        /// 
        /// Since the number of items is unknown,
        /// as tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="source">Note: If the source is a Collection, then the Count will be used and the estimatedSteps will be ignored.</param>
        /// <param name="estimatedCount">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        public static IEnumerable<T> WithProgressUnknown<T>(this IEnumerable<T> source, float estimatedCount, float estimatedWeight)
        {
            return ProgressExtensions.WithProgressUnknown(source, estimatedCount, estimatedWeight, null, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// 
        /// Since the number of items is unknown,
        /// as tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="source">Note: If the source is a Collection, then the Count will be used and the estimatedSteps will be ignored.</param>
        /// <param name="estimatedCount">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        /// <param name="taskArg">Provides additional info about the task being performed</param>
        public static IEnumerable<T> WithProgressUnknown<T>(this IEnumerable<T> source, float estimatedCount, float estimatedWeight, string taskKey, object taskArg)
        {
            return ProgressExtensions.WithProgressUnknown(source, estimatedCount, estimatedWeight, taskKey, taskArg, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// 
        /// Since the number of items is unknown,
        /// as tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="source">Note: If the source is a Collection, then the Count will be used and the estimatedSteps will be ignored.</param>
        /// <param name="estimatedCount">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <param name="taskKey">Identifies the task being performed.  Can be used for displaying progress.</param>
        public static IEnumerable<T> WithProgressUnknown<T>(this IEnumerable<T> source, float estimatedCount, float estimatedWeight, string taskKey)
        {
            return ProgressExtensions.WithProgressUnknown(source, estimatedCount, estimatedWeight, taskKey, null, null, 0);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// 
        /// Since the number of items is unknown,
        /// as tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="source">Note: If the source is a Collection, then the Count will be used and the estimatedSteps will be ignored.</param>
        /// <param name="estimatedCount">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        public static IEnumerable<T> WithProgressUnknown<T>(this IEnumerable<T> source, float estimatedCount, float estimatedWeight, ProgressChangedHandler callback)
        {
            return ProgressExtensions.WithProgressUnknown(source, estimatedCount, estimatedWeight, null, null, callback, int.MaxValue);
        }
        /// <summary> Tracks progress as the source is enumerated.
        /// 
        /// Since the number of items is unknown,
        /// as tasks complete, the progress will get nearer completion,
        /// but will never reach 100%.
        /// </summary>
        /// <param name="source">Note: If the source is a Collection, then the Count will be used and the estimatedSteps will be ignored.</param>
        /// <param name="estimatedCount">
        /// Determines how "unknown" progress is calculated.  This should be a rough estimate of the number of steps expected.
        /// As steps are completed, progress gets closer to 100%, but never reaches it.
        /// </param>
        /// <param name="estimatedWeight">
        /// A value between 0.0 and 1.0 that determines how much weight to place on the estimated steps.
        /// For example, if estimatedSteps is 100 and estimatedWeight is .75,
        /// then when 100 steps have completed, progress will be at 75%.
        /// 
        /// This value cannot equal 0.0 or 1.0.
        /// </param>
        /// <param name="callback">Attach a callback to the ProgressChanged event</param>
        /// <param name="maximumDepth">
        /// The maximum depth that will activate the callback.
        /// A value of 0 indicates that only this task will activate the callback.
        /// Default is int.MaxValue.
        /// </param>
        public static IEnumerable<T> WithProgressUnknown<T>(this IEnumerable<T> source, float estimatedCount, float estimatedWeight, ProgressChangedHandler callback, int maximumDepth)
        {
            return ProgressExtensions.WithProgressUnknown(source, estimatedCount, estimatedWeight, null, null, callback, maximumDepth);
        }
    }
}
