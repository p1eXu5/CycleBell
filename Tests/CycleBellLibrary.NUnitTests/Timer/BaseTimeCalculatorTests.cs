using System;
using System.Collections.Generic;
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
        public void GetTimerQueue_PresetContainsTimePoint_ReturnsExpectedQueue()
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
                    (TimeSpan.Parse ("12:00:10"), new TimePoint("Test point 1", "0:00:10", TimePointType.Relative, 10) {BaseTime = TimeSpan.Parse ("12:00:00")}),
                    (TimeSpan.Parse ("12:00:20"), new TimePoint("Test point 2", "12:00:20", TimePointType.Absolute, 20) {BaseTime = TimeSpan.Parse ("12:00:00")}),
                });

            Assert.That (btc.GetTimerQueue(preset), Is.EquivalentTo (expectedQueue));
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
