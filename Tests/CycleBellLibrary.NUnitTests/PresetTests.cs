using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests
{
    [TestFixture]
    public class PresetTests
    {
        [Test]
        public void AddTimePoint_TimePoint_AddsTimePoint()
        {
            var preset = new Preset();
            var timePoint = new TimePoint();

            preset.AddTimePoint(timePoint);

            Assert.IsTrue(preset.TimePoints.Count > 0);
        }

        [Test]
        public void GetTimePoints_TimePointsAreFilled_ReturnsOrdered()
        {
            var preset = new Preset();

            TimePoint[] timePoints = new[] {
                new TimePoint() { LoopNumber = 1 },
                new TimePoint() { LoopNumber = 2 }, // LoopNumber has increased
                new TimePoint() { LoopNumber = 2 }, // LoopNumber reminded the same
                new TimePoint() { LoopNumber = 1 }, // LoopNumber has decreased
            };

            int[] expectedOrder = { 0, 3, 1, 2 };
            
            
        }

        

        private TimePoint GetTimePointWithNoSetBaseTime()
        {
            var timePoint = new TimePoint("Test TimePoint", "0:00:30");
            timePoint.Name += timePoint.Id;

            return timePoint;
        }
    }
}
