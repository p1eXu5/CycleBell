using System.Collections.Generic;
using CycleBell.Engine.Models;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
using Moq;
using NUnit.Framework;

namespace CycleBell.Tests.UnitTests.ViewModels
{
    [TestFixture]
    class TimePointViewModelTests
    {
        #region RemoveTimePointCommand

        [Test]
        public void RemoveTimePointCommand_WhenCalled_RemovesTimePoint()
        {
            // Arrange
            var tpvm = GetTimePointViewModel();

            // Act
            tpvm.RemoveTimePointCommand.Execute (null);

            // Assert
            Assert.IsFalse (mockPreset.TimePoints.Contains (tpvm.TimePoint));
        }

        #endregion

        #region MuteToggleCommand

        [Test]
        public void MuteToggleCommand_MuteFlagIsTrue_SetsMuteFlagInFalse()
        {
            var tpvm = GetTimePointViewModel();
            tpvm.MuteFlag = true;

            tpvm.MuteToggleCommand.Execute (null);

            Assert.IsFalse (tpvm.MuteFlag);
        }

        [Test]
        public void MuteToggleCommand_MuteFlagIsFalse_SetsMuteFlagInTrue()
        {
            var tpvm = GetTimePointViewModel();
            tpvm.MuteFlag = false;

            tpvm.MuteToggleCommand.Execute (null);

            Assert.IsTrue (tpvm.MuteFlag);
        }

        #endregion

        #region ChangeTimpePointTypeCommand



        #endregion


        #region Factory

        private readonly Mock<IPresetViewModel> _mockPresetViewModel = new Mock<IPresetViewModel>();
        private readonly FakePreset mockPreset = new FakePreset();

        private TimePointViewModel GetTimePointViewModel()
        {
            TimePoint tp = TimePoint.GetAbsoluteTimePoint();

            return new TimePointViewModel (tp, _mockPresetViewModel.Object);
        }

        #endregion

        #region Fakes

        internal class FakePreset : Preset
        {
            private readonly List<TimePoint> _timePoints = new List<TimePoint>();

            public override void CheckTime (TimePoint timePoint)
            {
                _timePoints.Add (timePoint);
            }

            public override void PreRemoveTimePoint (TimePoint timePoint)
            {
                _timePoints.Remove (timePoint);
            }

            public List<TimePoint> TimePoints => _timePoints;
        }

        #endregion
    }
}
