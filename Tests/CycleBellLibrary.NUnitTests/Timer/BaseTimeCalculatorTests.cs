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

            do {
                Assert.IsTrue(expectedQueue.Dequeue().Item2 == actualQueue.Dequeue().Item2);

            } while (expectedQueue.Any());

            // Working with Object.Equals, not equality operator
            // Assert.That (actualQueue, Is.EquivalentTo (expectedQueue));
        }
    
        #region Factory

        public string StartTimeTimePointName => "Test Start TimePoint";

        private BaseTimeCalculator GetBaseTimeCalculator()
        {
            var btc = new BaseTimeCalculator (this);

            return btc;
        }

        #endregion
    }
}
