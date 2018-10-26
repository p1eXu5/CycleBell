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

            var queue = GetQueue().GetTimerQueue(preset);
            var absoluteTimes = GetAbsoluteTimes(queue);


        }

        #region Factory

        public string StartTimeTimePointName { get; } = "StartTime";
        private readonly Mock<ITimerManager> _mockTimerManager = new Mock<ITimerManager>();

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
            CycleBellManager cycleBellManager = new CycleBellManager(null, new PresetCollectionManager(), _mockTimerManager.Object);

            var mainViewModel = new MainViewModel(stub_DialogRegistrator.Object, cycleBellManager);

            cycleBellManager.AddPreset(preset);

            return mainViewModel.SelectedPreset;
        }

        #endregion

    }
}
