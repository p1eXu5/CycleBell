using System;
using CycleBell.Base;
using NUnit.Framework;

namespace CycleBell.ViewModels.NUnitTests
{
    [TestFixture]
    public class MainViewModelTests
    {
        [SetUp]
        public void TestInitializer()
        {}

        [Test]
        public void MainViewModel_TypeCheck_IsObservableObject()
        {
            Type actualType = typeof(MainViewModel).BaseType;
            Type expectedType = typeof(ObservableObject);

            Assert.AreEqual(expectedType, actualType);
        }

        [Test]
        public void CreatePresetCommand_WhenExecuted_AddsPreset()
        {

        }

        private MainViewModel MakeMainViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
