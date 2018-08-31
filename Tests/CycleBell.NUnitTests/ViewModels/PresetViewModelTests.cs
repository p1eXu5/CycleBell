using System;
using System.Linq;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBellLibrary;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.ViewModels
{
    [TestFixture]
    public class PresetViewModelTests
    {
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
            pvm.AddingTimePoint = new TimePointViewModel(new TimePoint(TimeSpan.Parse (time), timePointType));

            Assert.IsFalse(pvm.AddTimePointCommand.CanExecute(null));
        }


        [TestCase("0:00:00", TimePointType.Absolute)]
        [TestCase("0:00:01", TimePointType.Relative)]
        public void AddTimePointCommand_PositiveOrZeroAbsoluteTimePoint_CanExecute(string time, TimePointType timePointType)
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint = new TimePointViewModel(new TimePoint(TimeSpan.Parse(time), timePointType));

            Assert.IsTrue(pvm.AddTimePointCommand.CanExecute(null));
        }

        [Test]
        public void AddTimePointCommand_CanExecuteLoopNumUnknown_AddsBeginTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);

            var begin = pvm.TimePoints.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).First();

            Assert.AreEqual (begin, typeof(BeginTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_CanExecuteLoopNumUnknown_AddsEndTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);

            var begin = pvm.TimePoints.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).Last();

            Assert.AreEqual (begin, typeof(EndTimePointViewModel));
        }

        [Test]
        public void AddTimePointCommand_CanExecuteLoopUnknown_AddsTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;

            pvm.AddTimePointCommand.Execute (null);
            pvm.AddTimePointCommand.Execute (null);

            var sortedTimePoints = pvm.TimePoints.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber).ToArray();

            Assert.AreEqual (sortedTimePoints[0], typeof(BeginTimePointViewModel));
            Assert.AreEqual (sortedTimePoints[1], typeof(TimePointViewModel));
            Assert.AreEqual (sortedTimePoints[2], typeof(TimePointViewModel));
            Assert.AreEqual (sortedTimePoints[3], typeof(TimePointViewModel));
        }

        #region Factory

        private PresetViewModel GetPresetViewModel()
        {
            var preset = GetTestPreset();
            return new PresetViewModel(preset);
        }

        private Preset GetTestPreset() => new Preset("Test preset");

        #endregion
    }
}
