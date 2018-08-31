using System;
using System.Collections.ObjectModel;
using CycleBell.Base;
using CycleBellLibrary;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using NUnit.Framework;

namespace CycleBell.ViewModels.NUnitTests
{
    [TestFixture]
    public class MainViewModelTests
    {
        //private readonly Mock<IPresetsManager> _mockPresetsManager = new Mock<IPresetsManager>();

        [SetUp]
        public void TestInitializer()
        {
        }

        [Test]
        public void class_TypeCheck_IsObservableObject()
        {
            Type actualType = typeof(MainViewModel).BaseType;
            Type expectedType = typeof(ObservableObject);

            Assert.AreEqual(expectedType, actualType);
        }
    }
}
