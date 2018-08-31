using System;
using CycleBell.Base;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
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

        [TestCase("-0:00:01", TimePointType.Relative, false)]
        [TestCase("-0:00:01", TimePointType.Absolute, false)]
        [TestCase("0:00:00", TimePointType.Relative, false)]
        public void AddTimePointCommand_InvalidTimePoints_CanNotExecute(string time, TimePointType timePointType, bool canExecute)
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint = new TimePointViewModel(new TimePoint(TimeSpan.Parse (time), timePointType));

            Assert.AreEqual(canExecute, pvm.AddTimePointCommand.CanExecute(null));
        }

        [Test]
        public void AddTimePointCommand_ValidTimePoint_AddsTimePoint()
        {
            var pvm = GetPresetViewModel();
            pvm.AddingTimePoint.Name = "New TimePoint";

            pvm.AddTimePointCommand.Execute (null);

            Assert.IsTrue (pvm.TimePoints.Count == 1);
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
