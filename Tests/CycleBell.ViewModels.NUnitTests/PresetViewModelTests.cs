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
        public void AddTimePointCommand_ValidTimePoint_AddsTimePoint()
        {
            var preset = GetTestPreset();
            var pvm = new PresetViewModel (preset);

            pvm.AddTimePointCommand.Execute (null);

            Assert.IsTrue (pvm.TimePoints.Count == 1);
        }

        #region Factory

        private Preset GetTestPreset() => new Preset("Test preset");

        #endregion
    }
}
