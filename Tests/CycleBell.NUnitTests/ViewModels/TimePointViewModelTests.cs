using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
