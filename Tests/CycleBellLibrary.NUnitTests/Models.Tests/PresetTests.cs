using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Repository.Tests
{
    [TestFixture]
    public class PresetTests
    {
        #region ctor

        [Test]
        public void ctor_ParameterlessCalled_PresetNameIsDefaultName()
        {
            var preset = new Preset();

            Assert.IsTrue (preset.PresetName == Preset.DefaultName);
        }

        [Test]
        public void ctor_ParameterlessCalled_TimePointsCollectionNotNull()
        {
            var preset = new Preset();

            Assert.NotNull (preset.TimePoints);
        }

        [Test]
        public void ctor_ParameterlessCalled_TimePointsCountContainsNothing()
        {
            var preset = new Preset();

            Assert.IsTrue (preset.TimePoints.Count == 0);
        }

        #endregion

        #region AddTimePoint

        [Test]
        public void AddTimePoint_ValidTimePoint_AddsTimePoint()
        {
            var preset = new Preset();
            var timePoint = new TimePoint();

            preset.AddTimePoint(timePoint);

            Assert.IsTrue(preset.TimePoints.Count > 0);
        }

        [Test]
        public void AddTimePoint_TimePointIsNull_Throws()
        {
            var preset = new Preset();

            Assert.That(() => preset.AddTimePoint (null), Throws.ArgumentNullException);
        }

        // TODO Next tests are failed

        [TestCase("0:00:00", "0:00:00", "0:00:00")]
        [TestCase("23:59:59", "23:59:59", "00:00:00")]
        [TestCase("0:00:10", "0:00:20", "0:00:10")]
        [TestCase("23:59:50", "0:00:00", "0:00:10")]
        [TestCase("23:59:55", "0:00:10", "0:00:15")]
        public void AddTimePoint_AbsoluteTimeBaseTimeIsNull_GetsCorrectRelativeTime(string startTime, string absoluteTime, string expectedRelativeTime)
        {
            Preset.DefaultStartTime = TimeSpan.Parse (startTime);
            var preset = new Preset();

            TimePoint.DefaultTime = TimeSpan.Parse (absoluteTime);
            TimePoint.DefaultTimePointType = TimePointType.Absolute;
            var timePoint = TimePoint.DefaultTimePoint;

            // Action
            preset.AddTimePoint (timePoint);

            // Assert
            Assert.AreEqual (TimeSpan.Parse (expectedRelativeTime), timePoint.GetRelativeTime());
        }


        [TestCase("0:00:00", "0:00:00", "0:00:00")]
        [TestCase("23:59:59", "0:00:00", "23:59:59")]
        [TestCase("0:00:10", "0:00:10", "0:00:20")]
        [TestCase("23:59:50", "0:00:10", "0:00:00")]
        [TestCase("23:59:55", "0:00:15", "0:00:10")]
        public void AddTimePoint_RelativeTimeBaseTimeIsNull_SetsBaseTime(string startTime, string relativeTime, string expectedAbsoluteTime)
        {
            // Arrange
            Preset.DefaultStartTime = TimeSpan.Parse (startTime);

            var preset = new Preset();

            TimePoint.DefaultTime = TimeSpan.Parse (relativeTime);
            TimePoint.DefaultTimePointType = TimePointType.Relative;
            
            var timePoint = TimePoint.DefaultTimePoint;

            // Action
            preset.AddTimePoint (timePoint);

            // Assert
            Assert.AreEqual (TimeSpan.Parse (expectedAbsoluteTime), timePoint.GetAbsoluteTime());
        }


        [TestCase("0:00:00", "0:00:10", "1:12:45", "0:00:00")]
        [TestCase("0:00:00", "0:00:00", "1:12:45", "0:00:00")]
        [TestCase("0:10:00", "0:00:10", "1:12:45", "0:10:00")]
        [TestCase("0:10:00", "0:00:00", "1:12:45", "0:10:00")]
        public void AddTimePoint__RelativeTimePoint_BaseTimeSetted_TimePointsEmpty__ChangesEveryoneBaseTime(string startTime, string relativeTime, string preBaseTime, string expectedBaseTime)
        {
            var preset = new Preset();
            preset.StartTime = TimeSpan.Parse (startTime);

            var timePoint = GetRelativeTimePoint(TimeSpan.Parse (relativeTime));
            timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

            preset.AddTimePoint (timePoint);

            Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[0].BaseTime);
        }
 

        [TestCase("0:00:00", "0:00:10", "1:12:45", "0:00:00")]
        [TestCase("0:00:00", "0:00:00", "1:12:45", "0:00:00")]
        [TestCase("0:10:00", "0:00:10", "1:12:45", "0:10:00")]
        [TestCase("0:10:00", "0:00:00", "1:12:45", "0:10:00")]
        public void AddTimePoint__AbsoluteTimePoint_BaseTimeSetted_TimePointsEmpty__ChangesEveryoneBaseTime(string startTime, string relativeTime, string preBaseTime, string expectedBaseTime)
        {
            var preset = new Preset();
            preset.StartTime = TimeSpan.Parse (startTime);

            var timePoint = GetAbsoluteTimePoint(TimeSpan.Parse (relativeTime));
            timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

            preset.AddTimePoint (timePoint);

            Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[0].BaseTime);
        }


        [TestCase("0:00:00", "0:00:10", "1:12:45", "1:00:00")]
        [TestCase("0:00:00", "0:00:00", "1:12:45", "1:00:00")]
        [TestCase("0:10:00", "0:00:10", "1:12:45", "1:00:00")]
        [TestCase("0:10:00", "0:00:00", "1:12:45", "1:00:00")]
        public void AddTimePoint__RelativeTimePoint_BaseTimeSetted_TimePointsWithOneAbsolute__ChangesBaseTimeNextAddedTimePoint
            (string startTime, string relativeTime, string preBaseTime, string expectedBaseTime)
        {
            var preset = new Preset();
            preset.StartTime = TimeSpan.Parse (startTime);
            preset.AddTimePoint (new TimePoint("Arrange", "1:00:00", TimePointType.Absolute));

            var timePoint = GetRelativeTimePoint (TimeSpan.Parse (relativeTime));
            timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

            preset.AddTimePoint (timePoint);

            Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[1].BaseTime);
        }


        [TestCase("0:00:00", "0:00:10", "1:12:45", "1:00:00")]
        [TestCase("0:00:00", "0:00:00", "1:12:45", "1:00:00")]
        [TestCase("0:10:00", "0:00:10", "1:12:45", "1:10:00")]
        [TestCase("0:10:00", "0:00:00", "1:12:45", "1:10:00")]
        public void AddTimePoint__AbsoluteTimePoint_BaseTimeSetted_TimePointsWithOneRelative__ChangesBaseTimeNextAddedTimePoint
            (string startTime, string absoluteTime, string preBaseTime, string expectedBaseTime)
        {
            var preset = new Preset();
            preset.StartTime = TimeSpan.Parse (startTime);
            preset.AddTimePoint (new TimePoint("Arrange", "1:00:00", TimePointType.Relative));

            var timePoint = GetAbsoluteTimePoint(TimeSpan.Parse (absoluteTime));
            timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

            preset.AddTimePoint (timePoint);

            Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[1].BaseTime);
        }

        #endregion

        #region RemoveTimePoint

        [Test]
        public void RemoveTimePoint_TimePointIsNull_Throws()
        {
            var preset = new Preset();
            var timePoint = GetNewTestTimePoint();
            preset.AddTimePoint (timePoint);

            Assert.That (() => preset.RemoveTimePoint (null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveTimePoint_ValidTimePoint_RemovesTimePoint()
        {
            var preset = new Preset();
            var timePoint = GetNewTestTimePoint();
            preset.AddTimePoint (timePoint);

            preset.RemoveTimePoint (timePoint);

            Assert.IsTrue (preset.TimePoints.Count == 0);
        }

        [Test]
        public void RemoveTimePoint_TimePointIsNotInCollection_Throws()
        {
            // Arrange
            var preset = new Preset();

            var addedTimePoint = GetNewTestTimePoint();
            preset.AddTimePoint (addedTimePoint);

            var notInCollectionTimePoint = GetNewTestTimePoint();

            // Action and Assertion
            Assert.That (() => preset.RemoveTimePoint (notInCollectionTimePoint), Throws.ArgumentException);
        }

        #endregion


        #region Factory

        private TimePoint GetNewTestTimePoint()
        {
            var timePoint = new TimePoint("Test TimePoint", "0:00:30");
            timePoint.Name += timePoint.Id;

            return timePoint;
        }

        private TimePoint GetAbsoluteTimePoint(TimeSpan time)
        {
            var timePoint = new TimePoint(time, TimePointType.Absolute);

            return timePoint;
        }

        private TimePoint GetRelativeTimePoint(TimeSpan time)
        {
            var timePoint = new TimePoint(time, TimePointType.Relative);

            return timePoint;
        }

        #endregion
    }
}
