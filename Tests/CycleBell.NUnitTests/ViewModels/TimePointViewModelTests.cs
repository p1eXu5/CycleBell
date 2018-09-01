using System.Collections.Generic;
using System.Media;
using CycleBell.ViewModels;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.ViewModels
{
    [TestFixture]
    class TimePointViewModelTests
    {
        #region Fields

        private readonly FakePreset mockPreset = new FakePreset();
    
        #endregion


        #region ctor

        [Test]
        public void ctor_WhenCalledInTests_CreatsValidSoundPlayer()
        {
            // Arrange and Action:
            var tpvm = GetTimePointViewModel();

            // Assert:
            Assert.NotNull (tpvm.SoundPlayer);
        }

        #endregion

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
            tpvm.SoundPlayer = new SoundPlayer();
            tpvm.MuteFlag = true;

            tpvm.MuteToggleCommand.Execute (null);

            Assert.IsFalse (tpvm.MuteFlag);
        }

        [Test]
        public void MuteToggleCommand_MuteFlagIsFalse_SetsMuteFlagInTrue()
        {
            var tpvm = GetTimePointViewModel();
            tpvm.SoundPlayer = new SoundPlayer();
            tpvm.MuteFlag = false;

            tpvm.MuteToggleCommand.Execute (null);

            Assert.IsTrue (tpvm.MuteFlag);
        }

        [Test]
        public void MuteToggleCommand_SoundPlayerIsNull_CanExecute()
        {
            var tpvm = GetTimePointViewModel();
            tpvm.SoundPlayer = null;

            Assert.IsFalse (tpvm.MuteToggleCommand.CanExecute(null));
        }

        #endregion

        [Test]
        public void RingCommand_SoundPlayerNotNull_CanExecute()
        {
            var tpvm = GetTimePointViewModel();

            Assert.IsTrue (tpvm.RingCommand.CanExecute(null));
        }


        #region Factory

        private TimePointViewModel GetTimePointViewModel()
        {
            TimePoint tp = TimePoint.DefaultTimePoint;
            mockPreset.AddTimePoint (tp);

            return new TimePointViewModel (tp, mockPreset);
        }

        #endregion

        #region Fakes

        internal class FakePreset : Preset
        {
            private readonly List<TimePoint> _timePoints = new List<TimePoint>();

            public override void AddTimePoint (TimePoint timePoint)
            {
                _timePoints.Add (timePoint);
            }

            public override void RemoveTimePoint (TimePoint timePoint)
            {
                _timePoints.Remove (timePoint);
            }

            public new List<TimePoint> TimePoints => _timePoints;
        }

        #endregion
    }
}
