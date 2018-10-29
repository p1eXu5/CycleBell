using System;
using System.Collections.Generic;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.Integrational_Tests
{
    [TestFixture]
    class PresetViewModelIntegrationalTests : IStartTimeTimePointName
    {
        [TestCase("0:00:00")]
        [TestCase("23:59:00")]
        public void OnTimePointChangedEventHandler_ChangesTimePointsBaseTimesCorrect(string startTime)
        {
            var preset = GetPreset(TimeSpan.Parse(startTime));
            var pvm = GetPresetViewModel(preset);

            Assert.IsNotNull (pvm);

            var queue = GetQueue().GetTimerQueue(preset);
            var absoluteTimes = GetAbsoluteTimes(queue);

            _mockTimerManager.Raise (t => t.ChangeTimePointEvent += null, new TimerEventArgs (null, timePoints[0].TimePoint, TimeSpan.Zero, null));
        }

        #region Factory

        public string StartTimeTimePointName { get; } = "StartTime";
        private Mock<ITimerManager> _mockTimerManager;
        private readonly PresetCollectionManager _presetCollectionManager = new PresetCollectionManager();

        private Preset GetPreset(TimeSpan startTime) => new Preset(new []
                                                        {
                                                            new TimePoint("Test TimePoint # 1", "0:01:00", TimePointType.Relative, 0),
                                                            new TimePoint("Test TimePoint # 2", "0:01:00", TimePointType.Relative, 0),
                                                            new TimePoint("Test TimePoint # 3", "0:01:00", TimePointType.Relative, 1),
                                                            new TimePoint("Test TimePoint # 4", "0:01:00", TimePointType.Relative, 1),
                                                        }) { StartTime = startTime, TimerLoops = { [0] = 2, [1] = 2 } };

        private ITimerQueueCalculator GetQueue() => new TimerQueueCalculator(this);

        private TimeSpan[][] GetAbsoluteTimes(Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)> queue)
        {
            return null;
        }

        private PresetViewModel GetPresetViewModel(Preset preset)
        {
            Mock<IDialogRegistrator> stub_DialogRegistrator = new Mock<IDialogRegistrator>();

            var mockCycleBellManager = new Mock<ICycleBellManager>();
            _mockTimerManager = mockCycleBellManager.As<ITimerManager>();

            mockCycleBellManager.Setup (m => m.PresetCollectionManager).Returns (_presetCollectionManager);
            mockCycleBellManager.Setup (m => m.TimerManager).Returns (_mockTimerManager.Object);

            var mainViewModel = new MainViewModel(stub_DialogRegistrator.Object, mockCycleBellManager.Object);

            _presetCollectionManager.Add (preset);

            return mainViewModel.SelectedPreset;
        }

        #endregion

    }
}
