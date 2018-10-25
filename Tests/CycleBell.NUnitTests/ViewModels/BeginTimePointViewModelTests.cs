using System;
using System.ComponentModel;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBellLibrary.Models;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.ViewModels
{
    [TestFixture]
    public class BeginTimePointViewModelTests
    {
        [Test]
        public void class_IsIDataErrorInfo()
        {
            var type = typeof(BeginTimePointViewModel);

            Assert.IsTrue(typeof(IDataErrorInfo).IsAssignableFrom(type));
        }

        [Test]
        public void class_ReadIDataErrorInfoError_Throws()
        {
            var btpvm = GetBeginTimePontViewModel();

            Assert.Throws(typeof(NotImplementedException), () =>
                                                           {
                                                               // ReSharper disable once UnusedVariable
                                                               var error = btpvm.Error;
                                                           });
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(Int32.MinValue)]
        public void IDataErrorInfo_InvalidLoopCount_ValideteLoopCountReturnsNotNull(Int32 loopCount)
        {
            var btpvm = GetBeginTimePontViewModel();
            btpvm.LoopCount = loopCount;

            Assert.NotNull(btpvm["LoopCount"]);
            Assert.AreEqual("Loop count must be greater then 0", btpvm["LoopCount"]);
        }

        [TestCase(1)]
        [TestCase(Int32.MaxValue)]
        public void IDataErrorInfoValidLoopCount_ValideteLoopCountReturnsNull(Int32 loopCount)
        {
            var btpvm = GetBeginTimePontViewModel();
            btpvm.LoopCount = loopCount;

            Assert.IsNull(btpvm["LoopCount"]);
            Assert.AreEqual(null, btpvm["LoopCount"]);
        }

        #region Factory

        private readonly Mock<IPresetViewModel> _mockPresetViewModel = new Mock<IPresetViewModel>();

        private BeginTimePointViewModel GetBeginTimePontViewModel()
        {
            _mockPresetViewModel.Setup(a => a.Preset).Returns(new Preset(new[] {new TimePoint(),}));

            var btpvm = new BeginTimePointViewModel(0, _mockPresetViewModel.Object);

            return btpvm;
        }

        #endregion
    }
}
