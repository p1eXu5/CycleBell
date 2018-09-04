﻿using System;
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

        [Test]
        public void AddTimePoint_TimePointExist_Throws()
        {
            var preset = new Preset();
            var tp = GetAbsoluteTimePoint();
            preset.AddTimePoint (tp);

            Assert.That(() => preset.AddTimePoint (tp), Throws.ArgumentException);
        }

        [Test]
        public void AddTimePoint__IdLessThenMaximumContained__AddTimePointLikeNewTimePoint()
        {
            var timePoint = TimePoint.DefaultTimePoint;
            var timePointId = timePoint.Id;
            var preset = new Preset(new []{new TimePoint {LoopNumber = 1}, });

            preset.AddTimePoint (timePoint);

            Assert.AreEqual (timePointId + 2, timePoint.Id);
        }

        [Test]
        public void AddTimePoint_AutoUpdateIsTrueByDefaultRelativeTimePoints_UpdatesTimePointBaseTime()
        {
            // Arrange
            var timePoints = new TimePoint[]
                {
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 1 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 3 }
                };

            var preset = new Preset(timePoints);

            var addingTimePoint = new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 2 };

            // Action
            preset.AddTimePoint (addingTimePoint);

            // Assert
            Assert.AreEqual (TimeSpan.Parse ("0:00:00"), timePoints[0].BaseTime);
            Assert.AreEqual (TimeSpan.Parse ("1:00:00"), addingTimePoint.BaseTime);
            Assert.AreEqual (TimeSpan.Parse ("2:00:00"), timePoints[1].BaseTime);
        }

        //[TestCase("0:00:00", "0:00:00", "0:00:00")]
        //[TestCase("23:59:59", "23:59:59", "00:00:00")]
        //[TestCase("0:00:10", "0:00:20", "0:00:10")]
        //[TestCase("23:59:50", "0:00:00", "0:00:10")]
        //[TestCase("23:59:55", "0:00:10", "0:00:15")]
        //public void AddTimePoint_AbsoluteTimeBaseTimeIsNull_GetsCorrectRelativeTime(string startTime, string absoluteTime, string expectedRelativeTime)
        //{
        //    Preset.DefaultStartTime = TimeSpan.Parse (startTime);
        //    var preset = new Preset();

        //    TimePoint.DefaultTime = TimeSpan.Parse (absoluteTime);
        //    TimePoint.DefaultTimePointType = TimePointType.Absolute;
        //    var timePoint = TimePoint.DefaultTimePoint;

        //    // Action
        //    preset.AddTimePoint (timePoint);

        //    // Assert
        //    Assert.AreEqual (TimeSpan.Parse (expectedRelativeTime), timePoint.GetRelativeTime());
        //}


        //[TestCase("0:00:00", "0:00:00", "0:00:00")]
        //[TestCase("23:59:59", "0:00:00", "23:59:59")]
        //[TestCase("0:00:10", "0:00:10", "0:00:20")]
        //[TestCase("23:59:50", "0:00:10", "0:00:00")]
        //[TestCase("23:59:55", "0:00:15", "0:00:10")]
        //[TestCase("01:01:01","01:01:01", "02:02:02")]
        //public void AddTimePoint_RelativeTimeBaseTimeIsNull_SetsBaseTime(string startTime, string relativeTime, string expectedAbsoluteTime)
        //{
        //    // Arrange
        //    Preset.DefaultStartTime = TimeSpan.Parse (startTime);

        //    var preset = new Preset();

        //    TimePoint.DefaultTime = TimeSpan.Parse (relativeTime);
        //    TimePoint.DefaultTimePointType = TimePointType.Relative;
            
        //    var timePoint = TimePoint.DefaultTimePoint;

        //    // Action
        //    preset.AddTimePoint (timePoint);

        //    // Assert
        //    Assert.AreEqual (TimeSpan.Parse (expectedAbsoluteTime), timePoint.GetAbsoluteTime());
        //}


        //[TestCase("0:00:00", "0:00:10", "1:12:45", "0:00:00")]
        //[TestCase("0:00:00", "0:00:00", "1:12:45", "0:00:00")]
        //[TestCase("0:10:00", "0:00:10", "1:12:45", "0:10:00")]
        //[TestCase("0:10:00", "0:00:00", "1:12:45", "0:10:00")]
        //public void AddTimePoint__RelativeTimePoint_BaseTimeSetted_TimePointsEmpty__ChangesEveryoneBaseTime(string startTime, string relativeTime, string preBaseTime, string expectedBaseTime)
        //{
        //    var preset = new Preset();
        //    preset.StartTime = TimeSpan.Parse (startTime);

        //    var timePoint = GetRelativeTimePoint(TimeSpan.Parse (relativeTime));
        //    timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

        //    preset.AddTimePoint (timePoint);

        //    Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[0].BaseTime);
        //}
 

        //[TestCase("0:00:00", "0:00:10", "1:12:45", "0:00:00")]
        //[TestCase("0:00:00", "0:00:00", "1:12:45", "0:00:00")]
        //[TestCase("0:10:00", "0:00:10", "1:12:45", "0:10:00")]
        //[TestCase("0:10:00", "0:00:00", "1:12:45", "0:10:00")]
        //public void AddTimePoint__AbsoluteTimePoint_BaseTimeSetted_TimePointsEmpty__ChangesEveryoneBaseTime(string startTime, string relativeTime, string preBaseTime, string expectedBaseTime)
        //{
        //    var preset = new Preset();
        //    preset.StartTime = TimeSpan.Parse (startTime);

        //    var timePoint = GetAbsoluteTimePoint(TimeSpan.Parse (relativeTime));
        //    timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

        //    preset.AddTimePoint (timePoint);

        //    Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[0].BaseTime);
        //}


        //[TestCase("0:00:00", "0:00:10", "1:12:45", "1:00:00")]
        //[TestCase("0:00:00", "0:00:00", "1:12:45", "1:00:00")]
        //[TestCase("0:10:00", "0:00:10", "1:12:45", "1:00:00")]
        //[TestCase("0:10:00", "0:00:00", "1:12:45", "1:00:00")]
        //public void AddTimePoint__RelativeTimePoint_BaseTimeSetted_TimePointsWithOneAbsolute__ChangesBaseTimeNextAddedTimePoint
        //    (string startTime, string relativeTime, string preBaseTime, string expectedBaseTime)
        //{
        //    var preset = new Preset();
        //    preset.StartTime = TimeSpan.Parse (startTime);
        //    preset.AddTimePoint (new TimePoint("Arrange", "1:00:00", TimePointType.Absolute));

        //    var timePoint = GetRelativeTimePoint (TimeSpan.Parse (relativeTime));
        //    timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

        //    preset.AddTimePoint (timePoint);

        //    Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[1].BaseTime);
        //}


        //[TestCase("0:00:00", "0:00:10", "1:12:45", "1:00:00")]
        //[TestCase("0:00:00", "0:00:00", "1:12:45", "1:00:00")]
        //[TestCase("0:10:00", "0:00:10", "1:12:45", "1:10:00")]
        //[TestCase("0:10:00", "0:00:00", "1:12:45", "1:10:00")]
        //public void AddTimePoint__AbsoluteTimePoint_BaseTimeSetted_TimePointsWithOneRelative__ChangesBaseTimeNextAddedTimePoint
        //    (string startTime, string absoluteTime, string preBaseTime, string expectedBaseTime)
        //{
        //    var preset = new Preset();
        //    preset.StartTime = TimeSpan.Parse (startTime);
        //    preset.AddTimePoint (new TimePoint("Arrange", "1:00:00", TimePointType.Relative));

        //    var timePoint = GetAbsoluteTimePoint(TimeSpan.Parse (absoluteTime));
        //    timePoint.BaseTime = TimeSpan.Parse (preBaseTime);

        //    preset.AddTimePoint (timePoint);

        //    Assert.AreEqual(TimeSpan.Parse (expectedBaseTime), preset.TimePoints[1].BaseTime);
        //}

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

        [Test]
        public void RemoveTimePoint_AutoUpdateIsTrueByDefaultRelativeTimePoints_UpdatesTimePointBaseTime()
        {
            // Arrange
            var timePoints = new TimePoint[]
                {
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 1 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 2 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 3 }
                };

            var preset = new Preset(timePoints);

            var removingingTimePoint = timePoints[1];

            // Action
            preset.RemoveTimePoint (removingingTimePoint);

            // Assert
            Assert.AreEqual (TimeSpan.Parse ("0:00:00"), timePoints[0].BaseTime);
            Assert.AreEqual (TimeSpan.Parse ("1:00:00"), timePoints[2].BaseTime);
        }

        [Test]
        public void RemoveTimePoint_AutoUpdateIsTrueByDefaultAbsoluteTimePoints_UpdatesTimePointBaseTime()
        {
            // Arrange
            var timePoints = new TimePoint[]
                {
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 1 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 3 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 2 }
                };

            var preset = new Preset(timePoints);

            var removingingTimePoint = timePoints[1];

            // Action
            preset.RemoveTimePoint (removingingTimePoint);

            // Assert
            Assert.AreEqual (TimeSpan.Parse ("0:00:00"), timePoints[0].BaseTime);
            Assert.AreEqual (TimeSpan.Parse ("1:00:00"), timePoints[2].BaseTime);
        }

        #endregion

        [Test]
        public void StartTimeSetter_AutoUpdateIsTrueByDefaultRelativeTimePoints_UpdatesTimePointBaseTime()
        {
            // Arrange
            var timePoints = new TimePoint[]
                {
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 1 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 2 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 3 }
                };

            var preset = new Preset(timePoints);

            // Action
            preset.StartTime = TimeSpan.Parse ("1:00:00");

            // Assert
            Assert.AreEqual (TimeSpan.Parse ("1:00:00"), timePoints[0].BaseTime);
            Assert.AreEqual (TimeSpan.Parse ("2:00:00"), timePoints[1].BaseTime);
            Assert.AreEqual (TimeSpan.Parse ("3:00:00"), timePoints[2].BaseTime);
        }

        [Test]
        public void GetDeepCopy_WhenCalled_CreatesDeepCopy()
        {
            // Arrage:
            TimePoint[] originTimePoints = new TimePoint[] { new TimePoint("1", new TimeSpan(1, 1, 1), TimePointType.Relative, 1) };

            var originPreset = new Preset(originTimePoints)
                {
                    PresetName = "Origin",
                    StartTime = new TimeSpan (1, 1, 1),
                    Tag = "origin"
                };


            originPreset.SetInfiniteLoop();

            // Actoin:
            var deepPreset = originPreset.GetDeepCopy();

            // Assert:
            Assert.AreNotSame (originPreset, deepPreset);
            Assert.AreNotSame (originPreset.TimePoints, deepPreset.TimePoints);
            Assert.AreNotSame (originPreset.TimerLoops, deepPreset.TimerLoops);
            Assert.AreNotSame (originPreset.Tag, deepPreset.Tag);

            originPreset.PresetName = "Else";
            Assert.AreNotEqual (originPreset.PresetName, deepPreset.PresetName);

            originPreset.StartTime = new TimeSpan(2, 0, 0);
            Assert.AreNotEqual (originPreset.StartTime, deepPreset.StartTime);

            originPreset.ResetInfiniteLoop();
            Assert.AreNotEqual (originPreset.IsInfiniteLoop, deepPreset.IsInfiniteLoop);
        }

        [Test]
        public void GetDeepCopy_AfterCall_SameTimePointsLinks()
        {
            // Arrage:
            TimePoint[] originTimePoints = new TimePoint[] { new TimePoint("1", new TimeSpan(1, 1, 1), TimePointType.Relative, 1) };

            var originPreset = new Preset(originTimePoints)
                {
                    PresetName = "Origin",
                    StartTime = new TimeSpan (1, 1, 1),
                    Tag = "origin"
                };


            originPreset.SetInfiniteLoop();

            // Actoin:
            var deepPreset = originPreset.GetDeepCopy();

            // Assert:
            Assert.AreSame (originPreset.TimePoints[0], deepPreset.TimePoints[0]);
        }

        [Test]
        public void GetDeepCopy_AfterCallThenAddTimePointToOrigin_NotSameAddedTimePointsLinks()
        {
            // Arrage:
            TimePoint[] originTimePoints = new TimePoint[] { new TimePoint("1", new TimeSpan(1, 1, 1), TimePointType.Relative, 1) };

            var originPreset = new Preset(originTimePoints)
                {
                    PresetName = "Origin",
                    StartTime = new TimeSpan (1, 1, 1),
                    Tag = "origin"
                };


            originPreset.SetInfiniteLoop();

            // Actoin:
            var deepPreset = originPreset.GetDeepCopy();
            originPreset.AddTimePoint (TimePoint.DefaultTimePoint);

            // Assert:
            Assert.IsTrue (originPreset.TimePoints.Count == 2);
            Assert.IsTrue (deepPreset.TimePoints.Count == 1);
        }

        [Test]
        public void GetOrderedTimePoints_WhenCalled_ReturnsOrderedByLoopNumberThanByIdTimePoints()
        {
            // Arrange
            var timePoints = new TimePoint[]
                {
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 1 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 3 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 1 },
                    new TimePoint { Time = TimeSpan.Parse ("1:00:00"), LoopNumber = 2 },
                };

            var preset = new Preset(timePoints);

            // Action
            var resArray = preset.GetOrderedTimePoints().ToArray();

            // Assert
            Assert.IsTrue (resArray[0].Id == 1);
            Assert.IsTrue (resArray[1].Id == 3);
            Assert.IsTrue (resArray[2].Id == 4);
            Assert.IsTrue (resArray[3].Id == 2);
        }


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

        private TimePoint GetAbsoluteTimePoint()
        {
            var timePoint = new TimePoint() {TimePointType = TimePointType.Absolute};

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
