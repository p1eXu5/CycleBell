using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Repository.Tests
{
    [TestFixture]
    public class PresetTests
    {
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

        #region Factory

        private TimePoint GetNewTestTimePoint()
        {
            var timePoint = new TimePoint("Test TimePoint", "0:00:30");
            timePoint.Name += timePoint.Id;

            return timePoint;
        }

        #endregion
    }
}
