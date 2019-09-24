using System.ComponentModel;
using CycleBell.Engine.Models;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using Moq;
using NUnit.Framework;

namespace CycleBell.Tests.UnitTests.ViewModels
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
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.GetAbsoluteTimePoint());
            var tp = tpvmb.TimePoint;

            Assert.That(tpvmb.Equals(tp));
        }

        [Test]
        public void Equals_NotEqualTimePoints_ReturnsFalse()
        {
            TimePoint.DefaultKind = TimePointKinds.Relative;

            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.GetAbsoluteTimePoint());
            var tp = TimePoint.GetAbsoluteTimePoint();

            tp.ChangeTimePointType(TimePointKinds.Absolute);

            Assert.IsFalse(tpvmb.Equals(tp));
        }

        [Test]
        public void Equals_CompareToNull_ReturnsFalse()
        {
            TimePointViewModelBase tpvmb = GetTimePointViewModel(TimePoint.GetAbsoluteTimePoint());

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
