using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CycleBellLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CycleBellLibraryTests
{
    /// <summary>
    /// Summary description for TimePointTests
    /// </summary>
    [TestClass]
    public class TimePointTests
    {
        [TestMethod]
        public void GetAbsoluteTime_ReturnsTimeWhenStartTimePassTimeIsAbsolute()
        {
            TimeSpan t = TimeSpan.FromMinutes(5);
            TimeSpan tStart = new TimeSpan(0, 0, 5, 0);

            TimePoint tPoint = new TimePoint(t, TimePointType.Absolute);

            Assert.AreEqual(tPoint.GetAbsoluteTime(tStart), t);
        }

        [TestMethod]
        public void GetAbsoluteTime_ReturnsTimeLowerThenAboutMidnightStartTimeWhenTimeIsRelative()
        {
            TimeSpan time = TimeSpan.FromMinutes(20);
            TimeSpan startTime = new TimeSpan(23, 59, 59);

            TimePoint timePoint = new TimePoint(time);

            Assert.IsTrue(timePoint.GetAbsoluteTime(startTime) < startTime);
        }

        [TestMethod]
        public void TimePoint_ReturnsRelativeTimeAndAbsoluteTime()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;
            TimePoint timePoint = new TimePoint(time, TimePointType.Absolute);

            TimeSpan startTime = time - TimeSpan.FromMilliseconds(1);

            Assert.AreEqual(timePoint.GetRelativeTime(startTime, true), TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(timePoint.GetAbsoluteTime(startTime), time);
        }

        [TestMethod]
        public void GetRelativeTime_ReturnsTimeEqualsInitialTimeWhenRelativeFlagIsSet()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;
            TimePoint timePoint = new TimePoint(time);

            //Assert.AreEqual(timePoint.GetRelativeTime(), time);

            TimeSpan startTime = DateTime.Now.TimeOfDay;

            Assert.AreEqual(timePoint.GetRelativeTime(startTime), time);
        }

        [TestMethod]
        public void GetRelativeTime_ReturnsTimeNotEqualsInitialTimeWhenAbsoluteFlagIsSet()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;
            Thread.Sleep(1);
            TimePoint timePoint = new TimePoint(time, TimePointType.Absolute);

            TimeSpan startTime = DateTime.Now.TimeOfDay;

            Assert.AreNotEqual(timePoint.GetRelativeTime(startTime, true), time);
            //Assert.AreEqual(timePoint.GetRelativeTime(), timePoint.GetAbsoluteTime());
            Assert.AreEqual(timePoint.GetAbsoluteTime(), TimeSpan.FromHours(24) - startTime + time);
        }

        [TestMethod]
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

        [TestMethod]
        public void TimePointChangeId()
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

    }
}
