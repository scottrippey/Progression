using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Progression.Tests
{
    [TestFixture]
    public class ProgressFixture
    {
        [DebuggerStepThrough]
        private void DoNothing(object withNothing) { }

        private float currentProgress;
        /// <summary> Asserts that the current progress matches the expected progress.
        /// Allows for a very small tolerance, due to possible floating-point errors.
        /// </summary>
        /// <param name="expected">The expected value of currentProgress, from 0.0 to 100.0.</param>
        [DebuggerStepThrough]
        private void AssertCurrentProgress(float expected)
        {
            const float tolerance = 0.001f;
            var difference = Math.Abs(expected - currentProgress);
            if (difference > tolerance)
            {
                Assert.Fail("The current progress is {1:00.000}% but {0:00.000}% was expected.", expected, currentProgress);
            }
            //Console.WriteLine("{0:000.000}%", currentProgress);
        }
        /// <summary> Updates the currentProgress with the new progress, 
        /// checks that the new progress is greater than the old progress,
        /// and checks that the new progress is less than 100%.
        /// </summary>
        [DebuggerStepThrough]
        private void AssertProgressIsGrowing(ProgressChangedInfo p)
        {
            var newProgress = p.TotalProgress * 100;
            Console.WriteLine("{0:00.00}% (+{1:0.00}%) - \"{2}\"", newProgress, newProgress - currentProgress, p[0].TaskKey);
            Assert.GreaterOrEqual(newProgress, currentProgress);
            Assert.LessOrEqual(newProgress, 100f, "Progress during an unknown progress should never exceed 100% but is {0:000.00}%", newProgress);
            currentProgress = newProgress;
        }


        private int[] tenItems
        {
            get
            {
                return new[]{1,2,3,4,5,6,7,8,9,10};
            }
        }


        [Test]
        public void TestNormal()
        {
            currentProgress = -1f;
            // Normal for-loop:
            using (Progress.BeginTask(10).UpdateTask(AssertProgressIsGrowing))
            {
                for (int i = 0; i < 10; i++)
                {
                    Progress.NextStep();
                    AssertCurrentProgress(0.0f + 10f*i);
                }
                Progress.EndTask();
            }
            AssertCurrentProgress(100f);
        }

        [Test]
        public void TestNested()
        {
            currentProgress = -1f;
            // Begin main task with 4 sections, each one taking longer:
            using (Progress.BeginTask(new[] { 10f, 20f, 30f, 40f }).UpdateTask(AssertProgressIsGrowing))
            {
                Progress.NextStep(); // Advance the main task
                // Normal for-loop:
                Progress.BeginTask(10);
                for (int i = 0; i < 10; i++)
                {
                    Progress.NextStep();
                    AssertCurrentProgress((0.0f) + i);
                }
                Progress.EndTask();

                Progress.NextStep(); // Advance the main task
                // "Iterate" an array of 20 items:
                var twenty = new[]{1f,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20};
                foreach (var i in twenty.WithProgress())
                {
                    AssertCurrentProgress((10f) + (i-1f));
                }

                Progress.NextStep(); // Advance the main task
                // "Iterate" a proportional array of 30 items:
                var thirty = new[]{1f,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30};
                foreach (var i in thirty.WithProgress(thirty))
                {
                    // Calculate the step progress:
                    var x = i - 1f;
                    x = 30f * x*(x+1f)/930f;
                    AssertCurrentProgress((10f + 20f) + x);
                }

                Progress.NextStep(); // Advance the main task
                // Normal for-loop, with a "using" block instead of EndTask.
                using (Progress.BeginTask(40))
                {
                    for (int i = 0; i < 40; i++)
                    {
                        Progress.NextStep();
                        AssertCurrentProgress(60f + i);
                    }
                    Progress.EndTask();
                }
                AssertCurrentProgress(100f);
                Progress.EndTask();
            }
            AssertCurrentProgress(100f);

        }

        [Test]
        public void TestNestedMethods()
        {
            currentProgress = -1f;
            using (Progress.BeginTask(new[] { 10f, 20f, 550f }).UpdateTask(AssertProgressIsGrowing))
            {
                Progress.NextStep();
                AssertCurrentProgress(0f);

                Iterate10();

                AssertCurrentProgress(100f*(10f/580f));
                Progress.NextStep();
                AssertCurrentProgress(100f*(10f/580f));

                Iterate20();

                AssertCurrentProgress(100f*(30f/580f));
                Progress.NextStep();
                AssertCurrentProgress(100f*(30f/580f));
                
                Iterate550();

                AssertCurrentProgress(100f);
                Progress.EndTask();
            }
            AssertCurrentProgress(100f);
        }

        public void Iterate20()
        {
            using (Progress.BeginTask(20))
            {
                for (int i = 0; i < 20; i++)
                {
                    Progress.NextStep();
                    DoNothing(i);
                }
                Progress.EndTask();
            }
        }

        public void Iterate10()
        {
            var items = new[] {new {X = 10}, new {X = 20}, new {X = 30}, new {X = 40}, new {X = 50}, new {X = 60}, new {X = 70}, new {X = 80}, new {X = 90}, new {X = 100}};
            foreach (var item in items.WithProgress())
            {
                DoNothing(item);
            }
        }

        public void Iterate550()
        {
            var items = new[] {new {X = 10}, new {X = 20}, new {X = 30}, new {X = 40}, new {X = 50}, new {X = 60}, new {X = 70}, new {X = 80}, new {X = 90}, new {X = 100}};
            var proportions = items.Select(i => (float)i.X).ToArray();
            foreach (var x in items.WithProgress(proportions))
            {
                using (Progress.BeginTask(x.X))
                {
                    for (int i = 0; i < x.X; i++)
                    {
                        Progress.NextStep();
                        DoNothing(i);
                    }
                    Progress.EndTask();
                }
            }
        }
       
        [Test]
        public void TestUnknown()
        {
            currentProgress = -1f;
            using (Progress.BeginTaskUnknown(100f, .75f).UpdateTask(AssertProgressIsGrowing))
            {
                var count = 200; // Do way more than expected to make sure progress doesn't go over 100%.
                Console.WriteLine("Performing {0} iterations", count);
                for (int i = 0; i < count; i++)
                {
                    Console.Write("#{0:00}: ", i);
                    Progress.NextStep();

                    if (i == 100)
                    {
                        AssertCurrentProgress(75f);
                    }

                }
                Console.WriteLine("Done ({0} total)", count);
                Progress.EndTask();
            }


        }

        [Test]
        public void TestUnknownTimer()
        {
            currentProgress = -1f;
            using (Progress.BeginTask(10f,80f,10f).UpdateTask(AssertProgressIsGrowing))
            {
                Progress.NextStep("Stall 1 second");
                Progress.BeginTask(10);
                for (int i = 0; i < 10; i++)
                {
                    Progress.NextStep();
                    System.Threading.Thread.Sleep(100);
                }
                Progress.EndTask();


                // Start a task that takes unknown time:
                Progress.NextStep("Unknown for 8+ seconds");
                using (Progress.BeginTaskUnknown(8f, .90f, 1000))
                {
                    System.Threading.Thread.Sleep(8000);
                    System.Threading.Thread.Sleep(3000); // Take some extra time!
                    Progress.EndTask();
                }


                Progress.NextStep("Stall another second");
                using (Progress.BeginTask(10))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Progress.NextStep();
                        System.Threading.Thread.Sleep(100);
                    }
                    Progress.EndTask();
                }
                Progress.EndTask();
            }
        }


        [Test]
        public void TestCallbackDepth()
        {
            var justFine = false;
            ProgressChangedHandler callback = (p) =>{
                                                  if (p.Any(pi => pi.TaskKey == "Too Deep!")) 
                                                      Assert.Fail("'Too Deep!' invoked a callback!");
                                                  if (p.Any(pi => pi.TaskKey == "Just fine"))
                                                      justFine = true;
                                              };

            var items = new[]{1,2,3,4,5};

            // Here, we set the callback with a maximum depth of 3:
            foreach (var zero in items.WithProgress(callback, 3))
            {
                foreach (var one in items.WithProgress())
                {
                    foreach (var two in items.WithProgress())
                    {
                        // This progress is 3-deep, so it should work Just fine:
                        foreach (var three in items.WithProgress("Just fine"))
                        {
                            // This progress is 4-deep, so it shouldn't fire the callback:
                            foreach (var four in items.WithProgress("Too Deep!"))
                            {
                                DoNothing(four);       
                            }
                        }
                    }
                }
            }

            if (justFine == false)
            {
                Assert.Fail("'Just fine' did not invoke a callback!");
            }
        }


        [Test]
        public void Test_ProgressWithError()
        {
            try
            {
                currentProgress = -1;
                foreach (var i in tenItems.WithProgress(AssertProgressIsGrowing))
                {
                    foreach (var j in tenItems.WithProgress())
                    {
                        if (i == 5 && j == 5)
                        {
                            AssertCurrentProgress(44f);
                            // Throw an error in the middle of this task:
                            throw new OperationCanceledException("Die Progress Die!");
                        }
                    }
                }
                Assert.Fail("Code shouldn't get to this point.");
            }
            catch (OperationCanceledException)
            {
                // Make sure the current progress hasn't changed since the error:
                AssertCurrentProgress(44f);
            }

        }
    
    }
}
