using System;
using System.Windows.Input;
using CycleBell.Engine.Models;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using Moq;
using NUnit.Framework;

namespace CycleBell.Tests.UnitTests.ViewModels
{
    [TestFixture]
    public class AddingTimePointViewModelTests
    {
        [Test]
        public void Reset_WhenCalled_ResetsNestedTimePoint()
        {
            var avm = GetAddingTimePointViewModel();

            avm.CopyFrom(GetTestRelativeTimePoint(7));

            var beforeResetTimePointId = avm.TimePoint.Id;

            avm.Reset();

            var afterResetTimePoint = avm.TimePoint;

            Assert.AreEqual(beforeResetTimePointId, afterResetTimePoint.Id);
            Assert.AreEqual(afterResetTimePoint.Name, String.Empty);
            Assert.AreEqual(afterResetTimePoint.Time, TimeSpan.Zero);
            Assert.AreEqual(afterResetTimePoint.Kind, TimePointKinds.Relative);
            Assert.AreEqual(afterResetTimePoint.LoopNumber, 0);
        }


        #region Factory

        private readonly Mock<IPresetViewModel> _mockPresetViewModel = new Mock<IPresetViewModel>();
        private Mock<ICommand> _mockCommand;

        private AddingTimePointViewModel GetAddingTimePointViewModel()
        {
            _mockCommand = _mockPresetViewModel.As<ICommand>();
            //_mockCommand.Setup(c => c.Execute(It.IsAny<Object>())).Callback(() => { });
            //_mockCommand.Setup(c => c.CanExecute(It.IsAny<Object>())).Callback(() => { });

            _mockPresetViewModel.Setup(c => c.AddTimePointCommand).Returns(_mockCommand.Object);

            var atpvm = new AddingTimePointViewModel(_mockPresetViewModel.Object);

            return atpvm;
        }

        private TimePoint GetTestRelativeTimePoint(byte loopNumber) => new TimePoint("Test TimePoint", "0:00:10", TimePointKinds.Relative, loopNumber);

        #endregion
    }
}
