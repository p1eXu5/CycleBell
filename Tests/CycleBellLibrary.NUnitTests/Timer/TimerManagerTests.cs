using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        [ Test ]
        public void GetStartTimePoint_EqualStartTime_GetsSameTimePoint ()
        {

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
            var preset = GetShortPreset();

            bool canRiseTimePointChanged = false;
            tm.TimePointChanged += (sender, args) => canRiseTimePointChanged = true;

            // Action:
            tm.Play( preset );

            Thread.Sleep( 2 );

            // Assert:
            Assert.That( canRiseTimePointChanged );
            tm.Stop();
        }

        [Test]
        public void Play_ShortPreset_RaisesTimePointChangedEventWithExpectedArgs()
        {
            // Arrange:
            var tm = GetTimerManager();
            var preset = GetShortPreset();
            var expectedList = new [] {
                TimerManager.GetStartTimePoint( _startTime ),
                _timePoints[ 0 ],
                _timePoints[ 1 ],
                _timePoints[ 2 ],
            };

            var actualList = new List< TimePoint >();

            tm.TimePointChanged += ( sender, args ) =>
                                   {
                                       actualList.Add( args.NextTimePoint );
                                   };

            // Action:
            tm.Play( preset );

            Thread.Sleep( _shortDecey );

            // Assert:
            Assert.That( actualList.Select( a => (a.Name, a.GetAbsoluteTime()) ), Is.EquivalentTo( expectedList.Select( e => (e.Name, e.GetAbsoluteTime()) ) ) );

            tm.Stop();
        }

        [Test]
        public void Play_LongPreset_RaisesTimePointChangedEventWithExpectedArgs()
        {
            // Arrange:
            var tm = GetTimerManager();
            var preset = GetLongPreset();
            var expectedList = new[] {
                TimerManager.GetStartTimePoint( _startTime ),
                _timePoints[ 0 ],
                _timePoints[ 1 ],
                _timePoints[ 2 ],
            };

            var actualList = new List<TimePoint>();

            tm.TimePointChanged += (sender, args) =>
                                   {
                                       actualList.Add(args.NextTimePoint);
                                   };

            // Action:
            tm.Play(preset);

            Thread.Sleep(_longDecey);

            // Assert:
            Assert.That(actualList.Select(a => (a.Name, a.GetAbsoluteTime())), Is.EquivalentTo(expectedList.Select(e => (e.Name, e.GetAbsoluteTime()))));
            tm.Stop();
        }

        #endregion



        #region Factory

        private TimeSpan _startTime;
        private TimePoint[] _timePoints;
        private int _shortDecey = 10000;
        private int _longDecey = 100_000;

        private TimerManager GetTimerManager()
        {
            var tm = TimerManager.Instance;
            return tm;
        }

        private Preset GetLongPreset ()
        {
            _startTime = DateTime.Now.TimeOfDay + TimeSpan.FromMilliseconds( 20_000 );
            _timePoints = new[] {
                new TimePoint( "test tp 1", _startTime + TimeSpan.FromMilliseconds( 40_000 ), TimePointType.Absolute ),
                new TimePoint( "test tp 2", _startTime + TimeSpan.FromMilliseconds( 60_000 ), TimePointType.Absolute ),
                new TimePoint( "test tp 3", _startTime + TimeSpan.FromMilliseconds( 80_000 ), TimePointType.Absolute ),
            };

            var preset =  new Preset {
                PresetName = "Test Preset",
                StartTime = _startTime,
            };

            preset.AddTimePoints( _timePoints );

            return preset;
        }

        private Preset GetShortPreset ()
        {
            _startTime = DateTime.Now.TimeOfDay + TimeSpan.FromMilliseconds( 1000 );
            _timePoints = new[] {
                new TimePoint( "test tp 1", _startTime + TimeSpan.FromMilliseconds( 2000 ), TimePointType.Absolute ),
                new TimePoint( "test tp 2", _startTime + TimeSpan.FromMilliseconds( 3000 ), TimePointType.Absolute ),
                new TimePoint( "test tp 3", _startTime + TimeSpan.FromMilliseconds( 4000 ), TimePointType.Absolute ),
            };

            var preset =  new Preset {
                PresetName = "Test Preset",
                StartTime = _startTime,
            };

            preset.AddTimePoints( _timePoints );

            return preset;
        }

        #endregion
    }
}
