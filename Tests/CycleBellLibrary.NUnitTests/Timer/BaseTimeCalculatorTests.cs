using System;
using System.Collections.Generic;
using System.Linq;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Timer
{
    [TestFixture]
    public class BaseTimeCalculatorTests : IStartTimeTimePointName
    {
        [Test]
        public void GetTimerQueue_PresetIsNull_NotThows()
        {
            var btc = GetBaseTimeCalculator();

            Assert.That (() => btc.GetTimerQueue(null), Throws.Nothing);
        }

        [Test]
        public void GetTimerQueue_PresetIsNull_ReturnsNull()
        {
            var btc = GetBaseTimeCalculator();

            Assert.That (() => btc.GetTimerQueue(null), Is.Null);
        }

        [Test]
        public void GetTimerQueue_PresetContainsTimePoint_ReturnsEqualTimePoints()
        {
            var btc = GetBaseTimeCalculator();
            var preset = new Preset (new[]
                {
                    new TimePoint("Test point 1", "0:00:10", TimePointType.Relative, 10),
                    new TimePoint("Test point 2", "12:00:20", TimePointType.Absolute, 20),
                }) { StartTime = TimeSpan.FromHours (12) };

            Queue<(TimeSpan, TimePoint)> expectedQueue = new Queue<(TimeSpan, TimePoint)>(new []
                {
                    (TimeSpan.Parse ("12:00:00"), new TimePoint(StartTimeTimePointName, "12:00:00", TimePointType.Absolute) {BaseTime = TimeSpan.Parse ("12:00:00")}),
                    (TimeSpan.Parse ("12:00:10"), preset.TimePointCollection[0]),
                    (TimeSpan.Parse ("12:00:20"), preset.TimePointCollection[1]),
                });

            var actualQueue = btc.GetTimerQueue (preset);

            var comparer = new TimePointComparer();

            do {
                Assert.That(expectedQueue.Dequeue().Item2, Is.EqualTo(actualQueue.Dequeue().Item2).Using<TimePoint>(comparer));

            } while (expectedQueue.Any());

            // Working with Object.Equals, not equality operator
            // Assert.That (actualQueue, Is.EquivalentTo (expectedQueue));
        }

        [Test]
        public void GetTimerQueue_TestPresets_ReturnsTestResults()
        {
            var btc = GetBaseTimeCalculator();
            var data = GetTestData();

            for (int i = 0; i < data.presets.Length; ++i) {

                var actualQueue = btc.GetTimerQueue(data.presets[i]);
                Assert.That(actualQueue.Count == data.results[i].Length);

                for (int j = 0; j < actualQueue.Count; ++j) {

                    Assert.That(actualQueue.Peek().nextChangeTime, Is.EqualTo(data.results[i][j]));
                    actualQueue.Enqueue(actualQueue.Dequeue());
                }
            }
        }



        #region Factory

        public string StartTimeTimePointName => "Test Start TimePoint";

        private TimerQueueCalculator GetBaseTimeCalculator()
        {
            var btc = new TimerQueueCalculator (this);

            return btc;
        }

        private (Preset[] presets, TimeSpan[][] results) GetTestData()
        {
            var presets = new Preset[2];
            var result = new TimeSpan[2][];

            // 0:
            presets[0] = new Preset (new[]
            {
                new TimePoint("0:00:00", TimePointType.Absolute),
                new TimePoint("0:00:00", TimePointType.Absolute),
                new TimePoint("0:00:00", TimePointType.Absolute, 1),
                new TimePoint("0:00:00", TimePointType.Absolute, 1),
            })
            {
                StartTime = TimeSpan.Parse("0:00:00"),
                TimerLoops = {[0] = 2, [1] = 2}
            };

            result[0] = Enumerable.Range(0, 9).Select(n => TimeSpan.Parse("0:00:00")).ToArray();

            // 1:
            presets[1] = new Preset(new[]
            {
                new TimePoint("23:59:00", TimePointType.Absolute),
                new TimePoint("1:00:00", TimePointType.Relative),
                new TimePoint("23:59:00", TimePointType.Relative, 1),
                new TimePoint("01:00:00", TimePointType.Absolute, 1),
            })
            {
                StartTime = TimeSpan.Parse("0:00:00"),
                TimerLoops = { [0] = 2, [1] = 2 }
            };

            result[1] = new []
            {
                TimeSpan.Parse("0:00:00"), 
                TimeSpan.Parse("23:59:00"), 
                TimeSpan.Parse("0:59:00"), 
                TimeSpan.Parse("23:59:00"), 
                TimeSpan.Parse("0:59:00"), 
                TimeSpan.Parse("0:58:00"), 
                TimeSpan.Parse("1:00:00"), 
                TimeSpan.Parse("0:59:00"), 
                TimeSpan.Parse("1:00:00"), 
            };

            return (presets, result);
        }

        #endregion
    }
}
