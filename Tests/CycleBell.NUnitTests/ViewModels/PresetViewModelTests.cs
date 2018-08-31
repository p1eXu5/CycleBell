using System;
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
        public void AddTimePointCommand_CanExecuteLoopNumUnknown_AddsMinBoundTimePointViewModels()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Time = TimeSpan.FromSeconds (1);
            pvm.AddingTimePoint.LoopNumber = 10;
            var minBoundTimePoint = new TimePointViewModelBase ();

            pvm.AddTimePointCommand.Execute (null);

            Assert.AreEqual (pvm.TimePoints[0].GetType(), typeof());
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
