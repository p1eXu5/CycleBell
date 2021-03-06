﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CycleBell.Engine.Models;
using CycleBell.Engine.Timer;
using NUnit.Framework;

namespace CycleBell.Engine.Tests.UnitTests.Timer
{
    [TestFixture]
    public class TimerManagerTests
    {
        [Test]
        public void GetStartTimePoint_ByDefault_CreatesTimePointWithZeroBaseTime()
        {
            var tp = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));

            Assert.That( tp.BaseTime == TimeSpan.Zero );
        }

        [Test]
        public void GetStartTimePoint_ByDefault_CreatesAbsoluteTimePoint()
        {
            var tp = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));

            Assert.That(tp.Kind == TimePointKinds.Absolute);
        }

        [ Test ]
        public void GetStartTimePoint_ByDefault_CreatesTimePointWithStartTimePointName()
        {
            var tp = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));

            Assert.That(tp.Name, Is.EqualTo( TimerManager.START_TIMEPOINT_NAME ));
        }

        [ Test ]
        public void GetStartTimePoint_ByDefault_ReturnsDifferentTimePoints()
        {
            var tp1 = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));
            var tp2 = TimerManager.GetStartTimePoint(TimeSpan.Parse("1:11:11"));

            Assert.False( object.ReferenceEquals( tp1, tp2 ) );
        }


        #region Play

        [Test]
        public void Play_PresetIsNull_EmitsStopEvent()
        {
            var tm = GetTimerManager();

            bool res = false;
            tm.TimerStopped += (sender, args) => res = true;

            tm.Play(null);

            Assert.AreEqual(true, res);
        }

        [Test]
        public void Play_PresetDoesNotContainAnyTimePoints_EmitsStopEvent()
        {
            var tm = GetTimerManager();
            var preset = new Preset();

            bool res = false;
            tm.TimerStopped += (sender, args) => res = true;

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

            Thread.Sleep( 5 );

            // Assert:
            Assert.That( canRiseTimePointChanged );
            tm.Stop();
        }

        [Test]
        public void Play_ShortPreset_RaisesTimePointChangedEventWithExpectedNextTimePoint()
        {
            // Arrange:
            var tm = GetTimerManager();
            var preset = GetShortPreset();
            var expectedList = new [] {
                TimerManager.GetStartTimePoint( _startTime ),
                _timePoints[ 0 ],
                _timePoints[ 1 ],
                _timePoints[ 2 ],
                TimerManager.GetStartTimePoint( _startTime ),
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
        }


        [Test]
        public void Play__ShortPreset_ChangeStartTimeAfterFirstPlay__RaisesTimePointChangedEventWithExpectedNextTimePoint()
        {
            // Arrange:
            var tm = GetTimerManager();
            var preset = GetShortPreset();
            var expectedList = new [] {
                TimerManager.GetStartTimePoint( _startTime ),
                _timePoints[ 0 ],
                _timePoints[ 1 ],
                _timePoints[ 2 ],
                TimerManager.GetStartTimePoint( _startTime ),
            };

            var actualList = new List< TimePoint >();

            tm.TimePointChanged += ( sender, args ) =>
                                   {
                                       actualList.Add( args.NextTimePoint );
                                   };

            tm.Play( preset );
            Thread.Sleep( _shortDecey );
            Assert.That( actualList.Select( a => (a.Name, a.GetAbsoluteTime()) ), Is.EquivalentTo( expectedList.Select( e => (e.Name, e.GetAbsoluteTime()) ) ) );

            // Action:
            preset.StartTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );

            // Assert:

        }


        [Test]
        public void Play_ShortPreset_RaisesTimePointChangedEventWithExpectedPrevTimePoint()
        {
            // Arrange:
            var tm = GetTimerManager();
            var preset = GetShortPreset();
            var expectedList = new [] {
                TimerManager.InitialTimePoint,
                TimerManager.GetStartTimePoint( _startTime ),
                _timePoints[ 0 ],
                _timePoints[ 1 ],
                _timePoints[ 2 ],
            };

            var actualList = new List< TimePoint >();

            tm.TimePointChanged += ( sender, args ) =>
                                   {
                                       actualList.Add( args.PrevTimePoint );
                                   };

            // Action:
            tm.Play( preset );

            Thread.Sleep( _shortDecey );

            // Assert:
            Assert.That( actualList.Select( a => (a.Name, a.GetAbsoluteTime()) ), Is.EquivalentTo( expectedList.Select( e => (e.Name, e.GetAbsoluteTime()) ) ) );
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
                TimerManager.GetStartTimePoint( _startTime ),
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
        private int _longDecey = 26_000;

        private TimerManager GetTimerManager()
        {
            var tm = TimerManager.Instance;
            return tm;
        }

        private Preset GetLongPreset ()
        {
            _startTime = DateTime.Now.TimeOfDay + TimeSpan.FromMilliseconds( 5_000 );
            _timePoints = new[] {
                new TimePoint( "test tp 1", _startTime + TimeSpan.FromMilliseconds( 10_000 ), TimePointKinds.Absolute ),
                new TimePoint( "test tp 2", _startTime + TimeSpan.FromMilliseconds( 15_000 ), TimePointKinds.Absolute ),
                new TimePoint( "test tp 3", _startTime + TimeSpan.FromMilliseconds( 20_000 ), TimePointKinds.Absolute ),
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
                new TimePoint( "test tp 1", _startTime + TimeSpan.FromMilliseconds( 2000 ), TimePointKinds.Absolute ),
                new TimePoint( "test tp 2", _startTime + TimeSpan.FromMilliseconds( 3000 ), TimePointKinds.Absolute ),
                new TimePoint( "test tp 3", _startTime + TimeSpan.FromMilliseconds( 4000 ), TimePointKinds.Absolute ),
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
