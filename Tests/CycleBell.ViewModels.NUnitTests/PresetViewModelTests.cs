using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Base;
using CycleBellLibrary.Repository;
using NUnit.Framework;

namespace CycleBell.ViewModels.NUnitTests
{
    [TestFixture]
    public class PresetViewModelTests
    {
        [Test]
        public void class_DerivedToObservableObject()
        {
            Assert.AreEqual (typeof(PresetViewModel).BaseType, typeof(ObservableObject));
        }

        [Test]
        public void AddTimePointCommand_TimePointWithDefaultName_CantExecute()
        {
            var pvm = GetPresetViewModel();

            Assert.IsFalse(pvm.AddTimePointCommand.CanExecute(null));
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
