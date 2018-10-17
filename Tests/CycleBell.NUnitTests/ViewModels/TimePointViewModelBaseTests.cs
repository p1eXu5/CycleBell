using System.ComponentModel;
using System.Linq;
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
        public void class_IsINotifyPropertyChanged()
        {
            var type = typeof(TimePointViewModelBase);

            Assert.IsTrue(typeof(INotifyPropertyChanged).IsAssignableFrom(type));
        }

        [Test]
        public void Equals_EqualTimePoints_ReturnsTrue()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            Assert.IsTrue(tpvmb.Equals(tp));
        }

        [Test]
        public void Equals_NotEqualTimePoints_ReturnsFalse()
        {
            TimePoint.DefaultTimePointType = TimePointType.Relative;

            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);
            var tp = TimePoint.DefaultTimePoint;

            tp.ChangeTimePointType(TimePointType.Absolute);

            Assert.IsFalse(tpvmb.Equals(tp));
        }

        [Test]
        public void Equals_CompareToNull_ReturnsFalse()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.DefaultTimePoint);

            Assert.IsFalse(tpvmb == null);
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
