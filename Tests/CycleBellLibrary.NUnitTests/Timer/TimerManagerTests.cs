using System;
using CycleBellLibrary.Models;
using CycleBellLibrary.Timer;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Timer
{
    [TestFixture]
    public class TimerManagerTests
    {
        [Test]
        public void GetStartTimePoint_ByDefault_CreatesZeroBaseTimeTimePoint()
        {
            var tp = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));

            Assert.That( tp.BaseTime == TimeSpan.Zero );
        }

        [Test]
        public void GetStartTimePoint_ByDefault_CreatesAbsoluteTimePoint()
        {
            var tp = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));

            Assert.That(tp.TimePointType == TimePointType.Absolute);
        }

        #region Play

        [Test]
        public void Play_PresetIsNull_EmitsStopEvent()
        {
            var tm = GetTimerManager();

            bool res = false;
            tm.TimerStopEvent += (sender, args) => res = true;

            tm.Play(null);

            Assert.AreEqual(true, res);
        }

        [Test]
        public void Play_PresetDoesNotContainAnyTimePoints_EmitsStopEvent()
        {
            var tm = GetTimerManager();
            var preset = new Preset();

            bool res = false;
            tm.TimerStopEvent += (sender, args) => res = true;

            tm.Play(preset);

            Assert.AreEqual(true, res);
        }

        [Test]
        public void Play_PresetIsNull_DoesNotEmitStartEvent()
        {
            var tm = GetTimerManager();

            bool res = false;
            tm.TimerStarted += (sender, args) => res = true;

            tm.Play(null);

            Assert.AreNotEqual(true, res);
        }

        [Test]
        public void Play_PresetDoesNotContainAnyTimePoints_DoesNotEmitStartEvent()
        {
            var tm = GetTimerManager();
            var preset = new Preset();

            bool res = false;
            tm.TimerStarted += (sender, args) => res = true;

            tm.Play(preset);

            Assert.AreNotEqual(true, res);
        }

        [Test]
        public void Play_ByDefault_RaisesTimePointChangedEvent()
        {
            // Arrange:
            var tm = GetTimerManager();
            var preset = GetPreset();

            bool canRiseTimePointChanged = false;
            tm.TimePointChanged += (sender, args) => canRiseTimePointChanged = true;

            // Action:
            tm.Play( preset );

            // Assert:
            Assert.That( canRiseTimePointChanged );
        }

        #endregion



        #region Factory

        private TimerManager GetTimerManager()
        {
            var tm = TimerManager.Instance;
            return tm;
        }

        private Preset GetPreset ()
        {
            var startTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );

            var preset =  new Preset {
                PresetName = "Test Preset",
                StartTime = startTime,
            };

            preset.AddTimePoints( new [] {
                new TimePoint( startTime + TimeSpan.FromSeconds( 1 ), TimePointType.Absolute ), 
            } );

            return preset;
        }

        #endregion
    }
}
