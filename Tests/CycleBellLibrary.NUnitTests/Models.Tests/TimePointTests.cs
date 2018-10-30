using System;
using CycleBellLibrary.Models;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Models.Tests
{
    [TestFixture]
    public class TimePointTests
    {
        #region ctor

        [Test]
        public void ctor_ByDefault_CreatsTimePointWithNullBaseTime()
        {
            var timePoint = new TimePoint();

            Assert.IsTrue (timePoint.BaseTime == null);
        }

        [Test]
        public void ctor_ByDefault_CreatsRelativeTimePoint()
        {
            var timePoint = new TimePoint();

            Assert.IsTrue (timePoint.TimePointType == TimePointType.Relative);
        }

        [Test]
        public void ctor_ByDefault_CreatsTimePointWithDefaultTime()
        {
            var timePoint = new TimePoint();

            Assert.IsTrue (timePoint.Time == TimePoint.DefaultTime);
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

        [Test]
        public void ctor_NameParamIsNull_CreatesDefaultNamedTimePoint()
        {
            var timePoint = new TimePoint(null, "0:00:00");

            Assert.That(timePoint.Name, Is.EqualTo(TimePoint.GetDefaultTimePointName(timePoint)));
        }

        [Test]
        public void ctor_NameParamIsEmpty_CreatesEmptyStringNamedTimePoint()
        {
            var timePoint = new TimePoint(String.Empty, "0:00:00");

            Assert.That(timePoint.Name, Is.EqualTo(String.Empty));
        }

        //[Test]
        //public void ctor_ReachedMaximumOfTimePoints_Throw()
        //{
        //    var ex = Assert.Catch<OverflowException>(() =>
        //        {
        //            for (int i = 1; i <= TimePoint.MaxId; ++i) {
        //                var timePoint = new TimePoint();
        //            } 
        //        });

        //    StringAssert.Contains ("Reached the maximum number of TimePointCollection", ex.Message);
        //}

        #endregion ctor

        #region Time & BaseTime

        [TestCase ("24:00:00")]
        [TestCase ("1:23:00:00")]
        [TestCase ("-1:23:00:00")]
        [TestCase ("-586:23:00:00")]
        public void TimeSetter_NewTimeValueWithDays_CropsTheDays(string newTimeStr)
        {
            var timePoint = TimePoint.GetAbsoluteTimePoint();
            var newTime = TimeSpan.Parse (newTimeStr);

            timePoint.Time = newTime;

            Assert.That (timePoint.Time.Days == 0);
        }

        [TestCase ("24:00:00")]
        [TestCase ("1:23:00:00")]
        [TestCase ("-1:23:00:00")]
        [TestCase ("-586:23:00:00")]
        public void BaseTimeSetter_NewTimeValueWithDays_CropsTheDays(string newTimeStr)
        {
            var timePoint = TimePoint.GetAbsoluteTimePoint();
            var newTime = TimeSpan.Parse (newTimeStr);

            timePoint.BaseTime = newTime;

            Assert.That (((TimeSpan)timePoint.BaseTime).Days == 0);
        }

        #endregion

        #region GetAbsoluteTime

        [Test]
        public void GetAbsoluteTime__ValidBaseTime_PreserveBaseTimeByDefaultIsTrue_TimePointIsAbsolute__PreservesBaseTime()
        {
            var timePoint = GetAbsoluteTimePoint();
            var newBaseTime = TimeSpan.Parse("1:11:11");

            timePoint.GetAbsoluteTime(newBaseTime);

            Assert.That(timePoint.BaseTime == newBaseTime);
        }

        [Test]
        public void GetAbsoluteTime__ValidBaseTime_PreserveBaseTimeByDefaultIsTrue_TimePointIsRelative__PreservesBaseTime()
        {
            var timePoint = GetRelativeTimePoint();
            var newBaseTime = TimeSpan.Parse("1:11:11");

            timePoint.GetAbsoluteTime(newBaseTime);

            Assert.That(timePoint.BaseTime == newBaseTime);
        }

        [Test]
        public void GetAbsoluteTime__ValidBaseTime_PreserveBaseTimeIsFalse_TimePointIsAbsolute__DoesntPreserveBaseTime()
        {
            var timePoint = GetAbsoluteTimePoint();
            var newBaseTime = TimeSpan.Parse("1:11:11");

            timePoint.GetAbsoluteTime(newBaseTime, false);

            Assert.That(timePoint.BaseTime == TimeSpan.Zero);
        }

        [Test]
        public void GetAbsoluteTime__ValidBaseTime_PreserveBaseTimeIsFalse_TimePointIsRelative__DoesntPreserveBaseTime()
        {
            var timePoint = GetRelativeTimePoint();
            var newBaseTime = TimeSpan.Parse("1:11:11");

            timePoint.GetAbsoluteTime(newBaseTime, false);

            Assert.That(timePoint.BaseTime == null);
        }

        [Test]
        public void GetAbsoluteTime_ByDefaultRelativeTimePointBaseTimeNotSet_Throws()
        {
            var timePoint = GetRelativeTimePoint();

            var ex = Assert.Catch<Exception>(() => timePoint.GetAbsoluteTime());

            StringAssert.Contains("BaseTime must be set", ex.Message);
        }

        [Test]
        public void GetAbsoluteTime_ByDefaultAbsoluteTimePointBaseTimeNotSet_ReturnsTime()
        {
            var timePoint = GetAbsoluteTimePoint();

            var actualAbsoluteTime = timePoint.GetAbsoluteTime();

            Assert.AreEqual(timePoint.Time, actualAbsoluteTime);
        }

        [Test]
        public void GetAbsoluteTime_ValidBaseTimeAbsoluteTimePointBaseTimeNotSet_ChangeBaseTime()
        {
            var timePoint = GetAbsoluteTimePoint();

            timePoint.GetAbsoluteTime(TimeSpan.FromHours (13));

            Assert.AreNotEqual (TimeSpan.Zero, timePoint.BaseTime);
        }

        [TestCase("0:00:30", "23:59:59", "0:00:29")]
        [TestCase("0:00:30", "23:59:29", "23:59:59")]
        [TestCase("0:00:30", "-23:59:30", "-23:59:00")]
        [TestCase("0:00:30", "0:00:00", "0:00:30")]
        [TestCase("23:59:59", "23:59:59", "23:59:58")]
        [TestCase("00:00:00", "23:59:59", "23:59:59")]
        [TestCase("-00:00:00", "23:59:59", "23:59:59")]
        public void GetAbsoluteTime_ValidBaseTimeRelativeTimePoint_ReturnsExpectedAbsoluteTime(string time, string baseTime, string expectedAbsoluteTime)
        {
            var timePoint = GetRelativeTimePoint(time);
            var expectedTime = TimeSpan.Parse (expectedAbsoluteTime);

            var actualAbsoluteTime = timePoint.GetAbsoluteTime(TimeSpan.Parse(baseTime));

            Assert.AreEqual(expectedTime, actualAbsoluteTime);
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
        public void AbsoluteTimePoint_CreatedWithZeroRelativeTime()
        {
            var tp = TimePoint.GetAbsoluteTimePoint();

            Assert.That(tp.GetRelativeTime() == TimeSpan.Zero);
        }


        [Test]
        public void Clone_WhenCalled_GetsClone()
        {
            var timePoint = GetRelativeTimePoint(7);
            timePoint.Tag = "";

            var timePointClone = timePoint.Clone();

            Assert.AreNotSame(timePoint, timePointClone);
            Assert.AreEqual(timePoint.Tag, timePointClone.Tag);
            Assert.AreNotEqual(timePoint.Id, timePointClone.Id);
            Assert.AreEqual(timePoint.Name, timePointClone.Name);
            Assert.AreEqual(timePoint.TimePointType, timePointClone.TimePointType);
            Assert.AreEqual(timePoint.LoopNumber, timePointClone.LoopNumber);
            Assert.AreEqual(timePoint.BaseTime, timePointClone.BaseTime);
        }

        #region Factory

        private TimePoint GetRelativeTimePoint()
        {
            var tp = new TimePoint("Test TimePoint");
            tp.ChangeTimePointType (TimePointType.Relative);

            return tp;
        }

        private TimePoint GetRelativeTimePoint (byte loopNumber)
        {
            var tp = GetRelativeTimePoint();
            tp.LoopNumber = loopNumber;

            return tp;
        }

        private TimePoint GetRelativeTimePoint (string time)
        {
            var tp = GetRelativeTimePoint();
            tp.Time = TimeSpan.Parse (time);

            return tp;
        }

        private TimePoint GetAbsoluteTimePoint()
        {
            var tp = new TimePoint("Test TimePoint");
            tp.ChangeTimePointType (TimePointType.Absolute);

            return tp;
        }

        #endregion
    }
}
