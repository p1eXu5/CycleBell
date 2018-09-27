using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBellLibrary.Models;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.ViewModels
{
    [TestFixture]
    public class TimePointViewModelBaseTests
    {
        [Test]
        public void Equality_EqualTimePoints_ReturnsTrue()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            Assert.IsTrue(tpvmb == tp);
        }

        [Test]
        public void Equality_NotEqualTimePoints_ReturnsFalse()
        {
            TimePoint.DefaultTimePointType = TimePointType.Relative;

            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            tp.TimePointType = TimePointType.Absolute;

            Assert.IsTrue(tpvmb != tp);
        }

        #region Factory

        private readonly Mock<IPresetViewModel> _mockMainViewModel = new Mock<IPresetViewModel>();

        private TimePointViewModel GetTimePointViewModel(TimePoint timePoint)
        {
            var tpvm = new TimePointViewModel(timePoint, _mockMainViewModel.Object);
            return tpvm;
        }

        #endregion
    }
}
