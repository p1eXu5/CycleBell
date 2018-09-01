using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Models.Tests
{
    [TestFixture]
    public class TimePointTests
    {
        [Test]
        public void TimePoint_AbsoluteTimePoint_CreatesNoSetBaseTime()
        {
            var timePoint = GetAbsoluteTimePoint();

            Assert.Null(timePoint.BaseTime);
        }

        [Test]
        public void TimePoint_RelativeTimePoint_CreatesNoSetBaseTime()
        {
            var timePoint = GetRelativeTimePoint();

            Assert.Null(timePoint.BaseTime);
        }

        [Test]
        public void GetAbsoluteTime_RelativeTimePointBaseTimeNotSet_Throws()
        {
            var timePoint = GetRelativeTimePoint();

            var ex = Assert.Catch<Exception>(() => timePoint.GetAbsoluteTime());

            StringAssert.Contains("BaseTime must be set", ex.Message);
        }

        [Test]
        public void GetAbsoluteTime_AbsoluteTimePointBaseTimeNotSet_ReturnsTime()
        {
            var timePoint = GetAbsoluteTimePoint();

            var actualAbsoluteTime = timePoint.GetAbsoluteTime();

            Assert.AreEqual(timePoint.Time, actualAbsoluteTime);
        }

        [TestCase("0:00:30", "23:59:59", "0:00:29")]
        [TestCase("0:00:30", "23:59:29", "23:59:59")]
        [TestCase("0:00:30", "-23:59:30", "-23:59:00")]
        [TestCase("0:00:30", "0:00:00", "0:00:30")]
        public void GetAbsoluteTime_RelativeTimePointValidBaseTime_ReturnsAbsoluteTime(string time, string baseTime, string expectedAbsoluteTime)
        {
            var timePoint = GetRelativeTimePoint();

            timePoint.Time = TimeSpan.Parse(time);
            var actualAbsoluteTime = timePoint.GetAbsoluteTime(TimeSpan.Parse(baseTime));

            Assert.AreEqual(TimeSpan.Parse(expectedAbsoluteTime), actualAbsoluteTime);
        }

        [Test]
        public void GetRelativeTime_AbsoluteTimePointBaseTimeNotSet_Throws()
        {
            var timePoint = GetAbsoluteTimePoint();

            var ex = Assert.Catch<Exception>(() => timePoint.GetRelativeTime());

            StringAssert.Contains("BaseTime must be set", ex.Message);
        }

        [Test]
        public void GetRelativeTime_RelativeTimePointBaseTimeNotSet_ReturnsTime()
        {
            var timePoint = GetRelativeTimePoint();

            var actualRelativeTime = timePoint.GetRelativeTime();

            Assert.AreEqual(timePoint.Time, actualRelativeTime);
        }

        [TestCase("0:00:30", "23:59:59", "0:00:31")]
        [TestCase("0:00:30", "23:59:29", "0:01:01")]
        [TestCase("0:00:30", "-23:59:30", "1.00:00:00")]
        [TestCase("0:00:30", "0:00:00", "0:00:30")]
        public void GetRelativeTime_AbsoluteTimePointValidBaseTime_ReturnsRelativeTime(string time, string baseTime, string expectedRelativeTime)
        {
            var timePoint = GetAbsoluteTimePoint();

            timePoint.Time = TimeSpan.Parse(time);
            var actualRelativeTime = timePoint.GetRelativeTime(TimeSpan.Parse(baseTime));

            Assert.AreEqual(TimeSpan.Parse(expectedRelativeTime), actualRelativeTime);
        }

        [Test]
        public void ChangeId_ByDefault_ChangesIds()
        {
            TimePoint t1 = new TimePoint();
            TimePoint t2 = new TimePoint();

            int t1Id = t1.Id;
            int t2Id = t2.Id;

            t1.ChangeId(t2);

            Assert.AreEqual(t1.Id, t2Id);
            Assert.AreEqual(t2.Id, t1Id);
            Assert.AreNotEqual(t1Id, t2Id);
        }

        [Test]
        public void TimePointInLinqTest()
        {
            TimePoint[] points = new[]
            {
                new TimePoint {LoopNumber = 0, Name = "A"},
                new TimePoint {LoopNumber = 1, Name = "B"},
                new TimePoint {LoopNumber = 0, Name = "D"},
                new TimePoint {LoopNumber = 1, Name = "C"}
            };

            IEnumerable<byte> query = points.OrderBy(t => t.LoopNumber).Select(t => t.LoopNumber).Distinct();

            IEnumerable<TimePoint> queryPoint = (from t in points
                                                 orderby t.Id ascending
                                                 select t).ToList();

            foreach (var timePoint in query) {

                int i = 0;

                foreach (var point in queryPoint) {

                    Assert.AreEqual(points[i], point);
                    ++i;
                }
            }
        }

        private TimePoint GetRelativeTimePoint()
        {
            return new TimePoint("0:00:30");
        }

        private TimePoint GetAbsoluteTimePoint()
        {
            return new TimePoint("0:00:30", TimePointType.Absolute);
        }
    }
}
