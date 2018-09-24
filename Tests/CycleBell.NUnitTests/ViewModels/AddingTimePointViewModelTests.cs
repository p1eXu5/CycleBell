﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBellLibrary.Models;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace CycleBell.NUnitTests.ViewModels
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
            Assert.AreEqual(afterResetTimePoint.TimePointType, TimePointType.Relative);
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

        private TimePoint GetTestRelativeTimePoint(byte loopNumber) => new TimePoint("Test TimePoint", "0:00:10", TimePointType.Relative, loopNumber);

        #endregion
    }
}
