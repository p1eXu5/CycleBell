using System;
using System.Linq;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBellLibrary;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.ViewModels
{
    [TestFixture]
    public class PresetViewModelTests
    {
        private readonly Mock<ITimerManager> _mockTimerManager = new Mock<ITimerManager>();

        [Test]
        public void class_DerivedToObservableObject()
        {
            Assert.AreEqual (typeof(PresetViewModel).BaseType, typeof(ObservableObject));
        }

        [TestCase("-0:00:01", TimePointType.Relative)]
        [TestCase("-0:00:01", TimePointType.Absolute)]
        [TestCase("0:00:00", TimePointType.Relative)]
        public void AddTimePointCommand_NegativeOrZeroRelativeTimePoint_CanNotExecute(string time, TimePointType timePointType)
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint = new TimePointViewModel(new TimePoint(TimeSpan.Parse (time), timePointType), pvm.Preset);

            Assert.IsFalse(pvm.AddTimePointCommand.CanExecute(null));
        }

        [TestCase("0:00:00", TimePointType.Absolute)]
        [TestCase("0:00:01", TimePointType.Relative)]
        public void AddTimePointCommand_PositiveOrZeroAbsoluteTimePoint_CanExecute(string time, TimePointType timePointType)
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint = new TimePointViewModel(new TimePoint(TimeSpan.Parse(time), timePointType), pvm.Preset);

            Assert.IsTrue(pvm.AddTimePointCommand.CanExecute(null));
        }

        [Test]
        public void AddTimePointCommand_CanExecuteLoopNumUnknown_AddsBeginTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);

            var begin = pvm.TimePointVmCollection.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).First();

            Assert.AreEqual (begin.GetType(), typeof(BeginTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_CanExecuteLoopNumUnknown_AddsEndTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);

            var begin = pvm.TimePointVmCollection.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).Last();

            Assert.AreEqual (begin.GetType(), typeof(EndTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_CanExecuteLoopUnknown_AddsTimePointViewModels()
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

        [Test]
        public void UpdateTimePointVmCollection_EventRaisedByNewTimePoint_Adds()
        {
            // Arrange
            var pvm = GetPresetViewModel();
            var preset = pvm.Preset;

            // Action
             preset.AddTimePoint (TimePoint.DefaultTimePoint);

            // Assert
            Assert.IsTrue (pvm.TimePointVmCollection.Count == 3);
        }


        // PlayCommand

        [Test]
        public void PlayCommand_TimePointsIsEmpty_CanNotExecuted()
        {
            var pvm = GetPresetViewModel();

            Assert.IsFalse (pvm.PlayCommand.CanExecute (null));
        }

        [Test]
        public void PlayCommand_TimePointsNotEmpty_CanExecuted()
        {
            var pvm = GetPresetViewModel();
            pvm.Preset.AddTimePoint (GetTestTimePoint());

            Assert.IsTrue (pvm.PlayCommand.CanExecute(null));
        }

        [Test]
        public void PlayCommand_WhenExecuted_CallsITimerManager()
        {
            var pvm = GetPresetViewModel();
            pvm.Preset.AddTimePoint (GetTestTimePoint());

            pvm.PlayCommand.Execute (null);

            _mockTimerManager.Verify(tm => tm.Play (It.IsAny<Preset>()));
        }


        // PouseCommand

        [Test]
        public void PouseCommand_TimerIsNotRunning_CanNotExecuted()
        {
            var pvm = GetPresetViewModel();
            _mockTimerManager.Setup (tm => tm.IsRunning).Returns (false);

            Assert.IsFalse (pvm.PouseCommand.CanExecute (null));
        }

        [Test]
        public void PouseCommand_TimerIsRunning_CanExecuted()
        {
            var pvm = GetPresetViewModel();
            _mockTimerManager.Setup (tm => tm.IsRunning).Returns (true);

            Assert.IsTrue (pvm.PouseCommand.CanExecute(null));
        }

        [Test]
        public void PouseCommand_WhenExecuted_CallsITimerManager()
        {
            var pvm = GetPresetViewModel();
            _mockTimerManager.Setup (tm => tm.IsRunning).Returns (true);

            pvm.PouseCommand.Execute (null);

            _mockTimerManager.Verify (tm => tm.Pouse());
        }

        #region Factory

        private PresetViewModel GetPresetViewModel()
        {
            var preset = GetEmptyTestPreset();
            return new PresetViewModel(preset, _mockTimerManager.Object);
        }

        private Preset GetEmptyTestPreset() => new Preset("Test preset");

        private TimePoint GetTestTimePoint() => new TimePoint("0:00:10");

        #endregion
    }
}
