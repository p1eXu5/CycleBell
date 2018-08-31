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
            Assert.Equals (typeof(PresetViewModel).BaseType, typeof(ObservableObject));
        }

        #region Factory

        private Preset GetTestPreset() => new Preset("Test preset");

        #endregion
    }
}
