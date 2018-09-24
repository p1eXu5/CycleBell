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

        #region OnTimePointCollectionChanged

        [Test]
        public void OnTimePointCollectionChanged_AddNewTimePoint_AddsTimePointViewModels()
        {
            // Arrange
            var pvm = GetPresetViewModel();
            var preset = pvm.Preset;

            // Action
             preset.AddTimePoint (TimePoint.DefaultTimePoint);

            // Assert
            Assert.IsTrue (pvm.TimePointVmCollection.Count == 3);
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
            Assert.AreNotEqual(afterAddingTimePoint.TimePointType, timePoint.TimePointType);
            Assert.AreNotEqual(afterAddingTimePoint.LoopNumber, timePoint.LoopNumber);
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

        #region RemoveTimePoint

        [Test]
        public void RemoveTimePointCommand_MultiTimePoint_ClearsTimePointViewModelCollection()
        {
            // Arrange
            var pvm = GetPresetViewModel();

            var timePoint1 = new TimePoint("0:00:02", TimePointType.Relative, 9);
            pvm.AddingTimePoint.CopyFrom(timePoint1);

            pvm.AddTimePointCommand.Execute(null);

            var timePoint2 = new TimePoint("0:00:04", TimePointType.Relative, 10);
            pvm.AddingTimePoint.CopyFrom(timePoint2);

            pvm.AddTimePointCommand.Execute(null);

            var timePoint3 = new TimePoint("0:00:06", TimePointType.Relative, 9);
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



        #region Factory

        private readonly Mock<IMainViewModel> _mockMainViewModel = new Mock<IMainViewModel>();

        private PresetViewModel GetPresetViewModel()
        {
            var preset = GetEmptyTestPreset();

            return new PresetViewModel(preset, _mockMainViewModel.Object);
        }

        private Preset GetEmptyTestPreset() => new Preset("Test preset");

        private TimePoint GetTestRelativeTimePoint() => new TimePoint("0:00:10", TimePointType.Relative);
        private TimePoint GetTestRelativeTimePoint(byte loopNumber) => new TimePoint("0:00:10", TimePointType.Relative, loopNumber);

        #endregion
    }
}
