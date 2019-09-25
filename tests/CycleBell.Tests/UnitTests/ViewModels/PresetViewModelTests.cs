using System;
using System.Collections.ObjectModel;
using System.Linq;
using CycleBell.Base;
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;
using CycleBell.Tests.UnitTests.Factories;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using Moq;
using NUnit.Framework;

namespace CycleBell.Tests.UnitTests.ViewModels
{
    [TestFixture]
    public class PresetViewModelTests
    {
        #region class & ctor tests

        [Test]
        public void class_DerivedToObservableObject()
        {
            Assert.AreEqual (typeof(PresetViewModel).BaseType, typeof(ObservableObject));
        }


        [ Test ]
        public void ctor_PresetIsNull_Throws()
        {
            Assert.That( () => GetPresetViewModel( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void ctor_IMainViewModelIsNull_Throws()
        {
            Assert.That( () => new PresetViewModel( PresetFactory.GetPresetWithSounds(), null ), Throws.ArgumentNullException );
        }

        [Test]
        public void ctor_ByDefault_CreatesAddingTimePointVmWithEmptyName()
        {
            var pvm = GetPresetViewModel();

            Assert.AreEqual (String.Empty, pvm.AddingTimePoint.Name);
        }

        [ Test ]
        public void ctor_PresetContainsTimePointsWithSound_AddsSoundsToTheAlarm()
        {
            var preset = PresetFactory.GetPresetWithSounds();

            var pvm = GetPresetViewModel( preset );

            _mockAlarm.Verify( a => a.AddSound( It.IsAny< TimePoint >() ), Times.Exactly( preset.TimePointCollection.Count ));
        }

        #endregion


        #region OnTimePointCollectionChanged tests

        [Test]
        public void OnTimePointCollectionChanged_AddNewTimePoint_AddsTimePointViewModels()
        {
            // Arrange
            var pvm = GetPresetViewModel();
            var preset = pvm.Preset;

            // Action
             preset.AddTimePoint (TimePoint.GetAbsoluteTimePoint());

            // Assert
            Assert.IsTrue (pvm.TimePointVmCollection.Count == 3);
        }

        #endregion


        #region AddTimePointCommand tests

        [TestCase("-0:00:01", TimePointKinds.Relative)]
        [TestCase("-0:00:01", TimePointKinds.Absolute)]
        [TestCase("0:00:00", TimePointKinds.Relative)]
        public void AddTimePointCommand_NegativeOrZeroRelativeTimePoint_CanNotExecute(string time, TimePointKinds timePointType)
        {
            var pvm = GetPresetViewModel();

            pvm.AddingTimePoint.TimePoint.ChangeTimePointType(timePointType);
            pvm.AddingTimePoint.Time = TimeSpan.Parse (time);

            Assert.IsFalse(pvm.AddTimePointCommand.CanExecute(null));
        }

        [TestCase("0:00:00", TimePointKinds.Absolute)]
        [TestCase("0:00:01", TimePointKinds.Relative)]
        public void AddTimePointCommand_PositiveOrZeroAbsoluteTimePoint_CanExecute(string time, TimePointKinds timePointType)
        {
            var pvm = GetPresetViewModel();

            pvm.AddingTimePoint.Time = TimeSpan.Parse (time);
            pvm.AddingTimePoint.TimePoint.ChangeTimePointType(timePointType);

            Assert.IsTrue(pvm.AddTimePointCommand.CanExecute(null));
        }

        [Test]
        public void AddTimePointCommand_TimePointNameless_AddsDefaultedNameTimePoint()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);

            pvm.AddTimePointCommand.Execute (null);

            Assert.AreEqual (TimePoint.GetDefaultName (pvm.TimePointVmCollection[0].TimePoint), pvm.TimePointVmCollection[0].TimePoint.Name);
        }
        
        // WhenExecuted

        [Test]
        public void AddTimePointCommand_WhenExecuted_AddsNewTimePointWithCopiedFields()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            var time = pvm.AddingTimePoint.Time;
            var type = pvm.AddingTimePoint.TimePointKinds;
            var id = pvm.AddingTimePoint.Id;

            pvm.AddTimePointCommand.Execute (null);

            TimePointViewModel actual = (TimePointViewModel) pvm.TimePointVmCollection[0];

            // ReSharper disable once ArrangeStaticMemberQualifier
            Assert.IsFalse (Object.ReferenceEquals (pvm.AddingTimePoint.TimePoint, actual.TimePoint));
            Assert.IsTrue (time == actual.Time);
            Assert.IsTrue (type == actual.TimePointKinds);
            Assert.IsTrue (id != actual.Id);
        }

        [Test]
        public void AddTimePointCommand_WhenExecuted_ResetsAddingTimePoint()
        {
            var pvm = GetPresetViewModel();
            var timePoint = GetTestRelativeTimePoint(7);
            pvm.AddingTimePoint.CopyFrom(timePoint);

            pvm.AddTimePointCommand.Execute(null);
            TimePoint afterAddingTimePoint = pvm.AddingTimePoint;

            Assert.AreNotEqual(afterAddingTimePoint.Id, timePoint.Id);
            Assert.AreNotEqual(afterAddingTimePoint.Name, timePoint.Name);
            Assert.AreNotEqual(afterAddingTimePoint.Time, timePoint.Time);
            Assert.AreEqual(afterAddingTimePoint.Kind, TimePointKinds.Relative);
            Assert.AreEqual(afterAddingTimePoint.LoopNumber, 0);
        }

        // KnownLoop

        [Test]
        public void AddTimePointCommand_KnownLoop_AddsOnlyOneTimePointViewModel()
        {
            var pvm = GetPresetViewModel();

            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;
            pvm.AddTimePointCommand.Execute (null);

            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;
            pvm.AddTimePointCommand.Execute (null);

            var tpvm = pvm.TimePointVmCollection.Where (t => t is TimePointViewModel).ToList();
            Assert.IsTrue (tpvm.Count == 2);
        }

        // LoopUnknown

        [Test]
        public void AddTimePointCommand_LoopUnknown_AddsBeginTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);

            var begin = pvm.TimePointVmCollection.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).First();

            Assert.AreEqual (begin.GetType(), typeof(BeginTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_LoopUnknownMultiAdding_AddsAllNeedsTimePointViewModelBases()
        {
            // Arrange
            var pvm = GetPresetViewModel();
            var timePoint1 = GetTestRelativeTimePoint(1);
            var timePoint2 = GetTestRelativeTimePoint(2);

            // Action
            pvm.AddingTimePoint.CopyFrom(timePoint1);
            pvm.AddTimePointCommand.Execute(null);

            pvm.AddingTimePoint.CopyFrom(timePoint2);
            pvm.AddTimePointCommand.Execute(null);

            // Assert

            var collection = pvm.TimePointVmCollection.OrderBy(tp => tp.Id).ThenBy(tp => tp.LoopNumber).ToArray();

            Assert.AreEqual(collection[0].GetType(), typeof(BeginTimePointViewModel));
            Assert.AreEqual(collection[1].GetType(), typeof(BeginTimePointViewModel));
            Assert.AreEqual(collection[2].GetType(), typeof(TimePointViewModel));
            Assert.AreEqual(collection[3].GetType(), typeof(TimePointViewModel));
            Assert.AreEqual(collection[4].GetType(), typeof(EndTimePointViewModel));
            Assert.AreEqual(collection[5].GetType(), typeof(EndTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_LoopUnknown_AddsEndTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);

            var begin = pvm.TimePointVmCollection.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).Last();

            Assert.AreEqual (begin.GetType(), typeof(EndTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_LoopUnknown_AddsTimePointViewModels()
        {
            var pvm = GetPresetViewModel();

            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;
            pvm.AddTimePointCommand.Execute (null);

            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;
            pvm.AddTimePointCommand.Execute (null);

            var sortedTimePoints = pvm.TimePointVmCollection.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).ToArray();

            Assert.AreEqual (sortedTimePoints[0].GetType(), typeof(BeginTimePointViewModel));
            Assert.AreEqual (sortedTimePoints[1].GetType(), typeof(TimePointViewModel));
            Assert.AreEqual (sortedTimePoints[2].GetType(), typeof(TimePointViewModel));
            Assert.AreEqual (sortedTimePoints[3].GetType(), typeof(EndTimePointViewModel));
        }

        #endregion


        #region RemoveTimePointCommand tests

        [Test]
        public void RemoveTimePointCommand_MultiTimePoint_ClearsTimePointViewModelCollection()
        {
            // Arrange
            var pvm = GetPresetViewModel();

            var timePoint1 = new TimePoint("0:00:02", TimePointKinds.Relative, 9);
            pvm.AddingTimePoint.CopyFrom(timePoint1);

            pvm.AddTimePointCommand.Execute(null);

            var timePoint2 = new TimePoint("0:00:04", TimePointKinds.Relative, 10);
            pvm.AddingTimePoint.CopyFrom(timePoint2);

            pvm.AddTimePointCommand.Execute(null);

            var timePoint3 = new TimePoint("0:00:06", TimePointKinds.Relative, 9);
            pvm.AddingTimePoint.CopyFrom(timePoint3);

            pvm.AddTimePointCommand.Execute(null);

            var coll = pvm.TimePointVmCollection.Where(t => t.GetType() == typeof(TimePointViewModel))
                          .Select(t => t.TimePoint).OrderBy(t => t.Id).ToArray();

            // Action
            pvm.RemoveTimePoint(coll[0]);
            pvm.RemoveTimePoint(coll[1]);
            pvm.RemoveTimePoint(coll[2]);

            // Assert
            Assert.IsTrue(pvm.TimePointVmCollection.Count == 0);
        }

        [Test]
        public void RemoveTimePointCommand_MultiTimePoint_RemovesNestedLoopNumberSet()
        {
            var pvm = GetPresetViewModel();
            var tp = GetTestRelativeTimePoint(7);

            pvm.AddingTimePoint.CopyFrom(tp);
            pvm.AddTimePointCommand.Execute(null);

            pvm.RemoveTimePoint(pvm.TimePointVmCollection[0].TimePoint);

            Assert.IsTrue(pvm.TimePointVmCollection.Count == 0);

            pvm.AddingTimePoint.CopyFrom(tp);
            pvm.AddTimePointCommand.Execute(null);

            Assert.IsTrue(pvm.TimePointVmCollection.Count == 3);
        }

        #endregion


        #region OnTimePointChanged tests

        [Test]
        public void OnTimePointChanged_NextTimePointNotNull_ChangesActiveTpvmb()
        {
            MainViewModel mvm = GetMockedMainViewModel();
            
            var preset = new Preset( new[] {new TimePoint("1", "0:00:01"), new TimePoint("2", "0:00:02")});
            
            _presetCollection.Add (preset);

            var timePoints = mvm.SelectedPreset.TimePointVmCollection.Where (t => t is TimePointViewModel).ToArray();

            Assert.IsFalse (timePoints[0].IsActive);
            Assert.IsFalse (timePoints[1].IsActive);

            _mockTimerManager.Raise (t => t.TimePointChanged += null, 
                                     new TimerEventArgs (null, timePoints[0].TimePoint, TimeSpan.Zero, null));

            Assert.IsTrue (timePoints[0].IsActive);
            Assert.IsFalse (timePoints[1].IsActive);

            _mockTimerManager.Raise (t => t.TimePointChanged += null, new TimerEventArgs (null, timePoints[1].TimePoint, TimeSpan.Zero, null));

            Assert.IsFalse (timePoints[0].IsActive);
            Assert.IsTrue (timePoints[1].IsActive);
        }


        [ Test ]
        public void OnTimePointChanged_TimePointEventArgsIsNull_NotThrows()
        {
            var pvm = GetPresetViewModel( PresetFactory.GetPresetWithSounds() );

            Assert.That( () => pvm.OnTimePointChanged( null, null ), Throws.Nothing );
        }

        [ Test ]
        public void OnTimePointChanged_TimerEventArgsIsNotNull_CallsAlarmLoadNextSound()
        {
            var preset = PresetFactory.GetPresetWithSounds();
            var pvm = GetPresetViewModel( preset );

            pvm.OnTimePointChanged( this, new TimerEventArgs( null, null, TimeSpan.Zero, null ) );

            _mockAlarm.Verify( a => a.LoadNextSound( It.IsAny<TimePoint>() ), Times.Once);
        }

        [ Test ]
        public void OnTimePointChanged_PrevIsInitialTimePoint_CallsAlarmLoadNextSound()
        {
            var preset = PresetFactory.GetPresetWithSounds();
            var pvm = GetPresetViewModel( preset );
            var startTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );
            var initialTimePoint = TimerManager.InitialTimePoint;
            var startTimePoint = TimerManager.GetStartTimePoint( startTime );

            pvm.OnTimePointChanged( this, new TimerEventArgs( initialTimePoint, startTimePoint, TimeSpan.FromSeconds( 1 ), startTime ) );

            _mockAlarm.Verify( a => a.LoadNextSound( It.IsAny<TimePoint>() ), Times.Once);
            _mockAlarm.Verify( a => a.Play(), Times.Never );
            _mockAlarm.Verify( a => a.PlayDefault(), Times.Never );
        }

        [ Test ]
        public void OnTimePointChanged_PrevIsStartTimePoint_RingOnStartTime_CallsAlarmLoadNextSound()
        {
            var preset = PresetFactory.GetPresetWithSounds();
            var pvm = GetPresetViewModel( preset );
            var startTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );
            var startTimePoint = TimerManager.GetStartTimePoint( startTime );

            pvm.OnTimePointChanged( this, new TimerEventArgs( startTimePoint, preset.TimePointCollection[0], TimeSpan.FromSeconds( 1 ), startTime ) );

            _mockAlarm.Verify( a => a.LoadNextSound( It.IsAny<TimePoint>() ), Times.Once);
            _mockAlarm.Verify( a => a.Play(), Times.Once );
            _mockAlarm.Verify( a => a.PlayDefault(), Times.Never );
        }

        [ Test ]
        public void OnTimePointChanged_PrevIsRegularTimePoint_RingOnStartTime_CallsAlarmLoadNextSound()
        {
            var preset = PresetFactory.GetPresetWithSounds();
            var pvm = GetPresetViewModel( preset );
            var startTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );

            pvm.OnTimePointChanged( this, new TimerEventArgs( preset.TimePointCollection[0], preset.TimePointCollection[1], TimeSpan.FromSeconds( 1 ), startTime ) );

            _mockAlarm.Verify( a => a.LoadNextSound( It.IsAny<TimePoint>() ), Times.Once);
            _mockAlarm.Verify( a => a.Play(), Times.Once );
            _mockAlarm.Verify( a => a.PlayDefault(), Times.Never );
        }

        [ Test ]
        public void OnTimePointChanged_PrevIsStartTimePoint_DoesNotRingOnStartTime_CallsAlarmLoadNextSound()
        {
            var preset = PresetFactory.GetPresetWithSounds();
            var pvm = GetPresetViewModel( preset, false );
            var startTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );
            var startTimePoint = TimerManager.GetStartTimePoint( startTime );

            pvm.OnTimePointChanged( this, new TimerEventArgs( startTimePoint, preset.TimePointCollection[0], TimeSpan.FromSeconds( 1 ), startTime ) );

            _mockAlarm.Verify( a => a.LoadNextSound( It.IsAny<TimePoint>() ), Times.Once);
            _mockAlarm.Verify( a => a.Play(), Times.Never );
            _mockAlarm.Verify( a => a.PlayDefault(), Times.Never );
        }

        [ Test ]
        public void OnTimePointChanged_NextIsStartTimePoint_DoesNotRingOnStartTime_CallsAlarmLoadNextSound()
        {
            var preset = PresetFactory.GetPresetWithSounds();
            var pvm = GetPresetViewModel( preset, false );
            var startTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 1 );
            var startTimePoint = TimerManager.GetStartTimePoint( startTime );

            pvm.OnTimePointChanged( this, new TimerEventArgs( preset.TimePointCollection[0], startTimePoint, TimeSpan.FromSeconds( 1 ), startTime ) );

            _mockAlarm.Verify( a => a.LoadNextSound( It.IsAny<TimePoint>() ), Times.Once);
            _mockAlarm.Verify( a => a.Play(), Times.Once );
            _mockAlarm.Verify( a => a.PlayDefault(), Times.Never );
        }

        #endregion



        #region fields

        private Mock< IAlarm > _mockAlarm;

        #endregion


        #region factory

        private Mock<ITimerManager> _mockTimerManager;
        private readonly PresetCollection _presetCollection = new PresetCollection();

        private PresetViewModel GetPresetViewModel( Preset preset, bool ringOnStartTime = true )
        {
            _mockAlarm = new Mock< IAlarm >();

            var mockMvm = new Mock< IMainViewModel >();
            mockMvm.Setup( m => m.Alarm ).Returns( _mockAlarm.Object );
            mockMvm.Setup( m => m.IsRingOnStartTime ).Returns( ringOnStartTime );

            return new PresetViewModel( preset, mockMvm.Object );
        }

        private PresetViewModel GetPresetViewModel()
        {
            var preset = GetEmptyTestPreset();

            return new PresetViewModel(preset, GetMockedMainViewModel());
        }

        private MainViewModel GetMockedMainViewModel()
        {
            var mockDialogRegistrator = new Mock<IDialogRegistrator>();
            var mockCycleBellManager = new Mock<ICycleBellManager>();

            _mockTimerManager = mockCycleBellManager.As< ITimerManager >();

            mockCycleBellManager.Setup (m => m.PresetCollection).Returns (_presetCollection);
            mockCycleBellManager.Setup (m => m.TimerManager).Returns (_mockTimerManager.Object);

            var stubAlarm = new Mock<IAlarm>();
            var scoll = new ObservableCollection<Uri>();
            stubAlarm.Setup( a => a.DefaultSoundCollection ).Returns( new ReadOnlyObservableCollection< Uri >( scoll ) );

            var mvm = new MainViewModel (mockDialogRegistrator.Object, mockCycleBellManager.Object, stubAlarm.Object);
            return mvm;
        }

        private Preset GetEmptyTestPreset() => new Preset("Test preset");

        private TimePoint GetTestRelativeTimePoint(byte loopNumber) => new TimePoint("0:00:10", TimePointKinds.Relative, loopNumber);

        #endregion
    }
}
