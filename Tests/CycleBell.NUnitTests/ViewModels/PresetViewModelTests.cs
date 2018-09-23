using System;
using System.Linq;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBellLibrary.Context;
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
        

        #region Class & ctor

        [Test]
        public void class_DerivedToObservableObject()
        {
            Assert.AreEqual (typeof(PresetViewModel).BaseType, typeof(ObservableObject));
        }

        [Test]
        public void ctor_WhenCreated_CreatesEmptyNamedTimePoint()
        {
            var pvm = GetPresetViewModel();

            Assert.AreEqual (String.Empty, pvm.AddingTimePoint.Name);
        }

        #endregion

        #region AddTimePointCommand

        [TestCase("-0:00:01", TimePointType.Relative)]
        [TestCase("-0:00:01", TimePointType.Absolute)]
        [TestCase("0:00:00", TimePointType.Relative)]
        public void AddTimePointCommand_NegativeOrZeroRelativeTimePoint_CanNotExecute(string time, TimePointType timePointType)
        {
            var pvm = GetPresetViewModel();

            pvm.AddingTimePoint.Time = TimeSpan.Parse (time);
            pvm.AddingTimePoint.TimePointType = timePointType;

            Assert.IsFalse(pvm.AddTimePointCommand.CanExecute(null));
        }

        [TestCase("0:00:00", TimePointType.Absolute)]
        [TestCase("0:00:01", TimePointType.Relative)]
        public void AddTimePointCommand_PositiveOrZeroAbsoluteTimePoint_CanExecute(string time, TimePointType timePointType)
        {
            var pvm = GetPresetViewModel();

            pvm.AddingTimePoint.Time = TimeSpan.Parse (time);
            pvm.AddingTimePoint.TimePointType = timePointType;

            Assert.IsTrue(pvm.AddTimePointCommand.CanExecute(null));
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

        [Test]
        public void AddTimePointCommand_TimePointNameless_AddsDefaultedNameTimePoint()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);

            pvm.AddTimePointCommand.Execute (null);

            Assert.AreEqual (pvm.TimePointVmCollection[0].TimePoint.GetDefaultTimePointName(), pvm.TimePointVmCollection[0].TimePoint.Name);
        }
        
        // WhenExecuted

        [Test]
        public void AddTimePointCommand_WhenExecuted_AddsNewTimePointWithCopiedFields()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            var time = pvm.AddingTimePoint.Time;
            var type = pvm.AddingTimePoint.TimePointType;
            var id = pvm.AddingTimePoint.Id;

            pvm.AddTimePointCommand.Execute (null);

            TimePointViewModel actual = (TimePointViewModel) pvm.TimePointVmCollection[0];

            Assert.IsFalse (Object.ReferenceEquals (pvm.AddingTimePoint.TimePoint, actual.TimePoint));
            Assert.IsTrue (time == actual.Time);
            Assert.IsTrue (type == actual.TimePointType);
            Assert.IsTrue (id != actual.Id);
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



        #region Factory

        private readonly Mock<IMainViewModel> _mockMainViewModel = new Mock<IMainViewModel>();

        private PresetViewModel GetPresetViewModel()
        {
            var preset = GetEmptyTestPreset();

            return new PresetViewModel(preset, _mockMainViewModel.Object);
        }

        private Preset GetEmptyTestPreset() => new Preset("Test preset");

        private TimePoint GetTestTimePoint() => new TimePoint("0:00:10");

        #endregion
    }
}
