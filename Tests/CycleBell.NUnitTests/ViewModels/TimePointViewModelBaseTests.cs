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
        public void EqualToOperator_EqualTimePoints_ReturnsTrue()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            Assert.IsTrue(tpvmb == tp);
        }

        [Test]
        public void EqualToOperator_NotEqualTimePoints_ReturnsFalse()
        {
            TimePoint.DefaultTimePointType = TimePointType.Relative;

            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            tp.TimePointType = TimePointType.Absolute;

            Assert.IsFalse(tpvmb == tp);
        }

        [Test]
        public void EqualToOperator_CompareToNull_ReturnsFalse()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);

            Assert.IsFalse(tpvmb == null);
        }

        [Test]
        public void NotEqualToOperator_NotEqualTimePoints_ReturnsFalse()
        {
            TimePoint.DefaultTimePointType = TimePointType.Relative;

            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            tp.TimePointType = TimePointType.Absolute;

            Assert.IsTrue(tpvmb != tp);
        }

        [Test]
        public void NotEqualToOperator_EqualTimePoints_ReturnsFalse()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            Assert.IsFalse(tpvmb != tp);
        }

        [Test]
        public void NorEqualToOperator_CompareToNull_ReturnsTrue()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);

            Assert.IsTrue(tpvmb != null);
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
