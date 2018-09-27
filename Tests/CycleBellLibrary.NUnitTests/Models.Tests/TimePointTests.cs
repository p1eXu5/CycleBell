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
        #region ctor

        [Test]
        public void ctor_ParameterlessCalled_CreatsTimePointWithNullBaseTime()
        {
            var timePoint = TimePoint.DefaultTimePoint;

            Assert.IsTrue (timePoint.BaseTime == null);
        }

        [Test]
        public void ctor_AbsoluteTimePoint_CreatesZeroBaseTime()
        {
            var timePoint = GetAbsoluteTimePoint();

            Assert.AreEqual(TimeSpan.Zero, timePoint.BaseTime);
        }

        [Test]
        public void ctor_RelativeTimePoint_CreatesNoSetBaseTime()
        {
            var timePoint = GetRelativeTimePoint();

            Assert.Null(timePoint.BaseTime);
        }

        #endregion ctor

        #region GetAbsoluteTime

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
        [TestCase("23:59:59", "23:59:59", "23:59:58")]
        [TestCase("00:00:00", "23:59:59", "23:59:59")]
        public void GetAbsoluteTime_RelativeTimePointValidBaseTime_ReturnsExpectedAbsoluteTime(string time, string baseTime, string expectedAbsoluteTime)
        {
            var timePoint = GetRelativeTimePoint();

            timePoint.Time = TimeSpan.Parse(time);
            var actualAbsoluteTime = timePoint.GetAbsoluteTime(TimeSpan.Parse(baseTime));

            Assert.AreEqual(TimeSpan.Parse(expectedAbsoluteTime), actualAbsoluteTime);
        }

        #endregion

        #region GetRelativeTime

        [Test]
        public void GetRelativeTime_AbsoluteTimePointBaseTimeNotSet_Throws()
        {
            var timePoint = GetAbsoluteTimePoint();
            timePoint.BaseTime = null;

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
        [TestCase("23:59:59", "23:59:59", "0:00:00")]
        public void GetRelativeTime_AbsoluteTimePointValidBaseTime_ReturnsRelativeTime(string time, string baseTime, string expectedRelativeTime)
        {
            var timePoint = GetAbsoluteTimePoint();

            timePoint.Time = TimeSpan.Parse(time);
            var actualRelativeTime = timePoint.GetRelativeTime(TimeSpan.Parse(baseTime));

            Assert.AreEqual(TimeSpan.Parse(expectedRelativeTime), actualRelativeTime);
        }

        #endregion

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
        public void Clone_WhenCalled_GetsClone()
        {
            var timePoint = GetRelativeTimePoint(7);

            var timePointClone = timePoint.Clone();

            Assert.AreNotSame(timePoint, timePointClone);
            Assert.AreNotEqual(timePoint.Id, timePointClone.Id);
            Assert.AreEqual(timePoint.Name, timePointClone.Name);
            Assert.AreEqual(timePoint.TimePointType, timePointClone.TimePointType);
            Assert.AreEqual(timePoint.LoopNumber, timePointClone.LoopNumber);
            Assert.AreEqual(timePoint.BaseTime, timePointClone.BaseTime);
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

        [Test]
        public void Equality_EqualTimePoints_ReturnsTrue()
        {
            var tp1 = TimePoint.DefaultTimePoint;
            var tp2 = TimePoint.DefaultTimePoint;

            Assert.AreNotSame(tp1, tp2);
            Assert.IsTrue(tp1 == tp2);
        }

        [Test]
        public void Equality_NotEqualTimePoints_ReturnsFalse()
        {
            TimePoint.DefaultTimePointType = TimePointType.Relative;

            var tp1 = TimePoint.DefaultTimePoint;
            var tp2 = TimePoint.DefaultTimePoint;
            tp2.TimePointType = TimePointType.Absolute;

            Assert.AreNotSame(tp1, tp2);
            Assert.IsTrue(tp1 != tp2);
        }

        #region Factory

        private TimePoint GetRelativeTimePoint()
        {
            return new TimePoint("0:00:30") {TimePointType = TimePointType.Relative};
        }

        private TimePoint GetRelativeTimePoint(byte loopNumber) => new TimePoint("Test Relative TimePoint", "0:00:07", TimePointType.Relative, loopNumber);

        private TimePoint GetAbsoluteTimePoint()
        {
            return new TimePoint("0:00:30", TimePointType.Absolute);
        }

        #endregion
    }
}
