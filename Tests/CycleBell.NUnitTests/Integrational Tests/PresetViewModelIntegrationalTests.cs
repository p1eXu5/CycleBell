using System;
using System.Collections.Generic;
using System.Linq;
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
        [Test]
        public void OnTimePointChangedEventHandler__InitialTimePointIsPrev_StartTimePointIsNext__DoesntActivateAnyTimePointViewModel()
        {
            var preset = GetPreset(TimeSpan.Zero);
            var pvm = GetPresetViewModel(preset);
            var queue = GetQueue().GetTimerQueue(preset).ToArray();

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs ( prevTimePoint: TimerManager.GetInitialTimePoint(),
                                                                                              nextTimePoint: queue[0].nextTimePoint,
                                                                                              lastTimeToNextChange: default(TimeSpan),
                                                                                              prevTimePointNextBaseTime: null
                                                                                             ));

            Assert.IsFalse(pvm.TimePointVmCollection.Any(tp => tp.IsActive));
        }

        [Test]
        public void OnTimePointChangedEventHandler__StartTimePointIsPrev_ExistedTimePointIsNext__ActivateTimePointViewModelEqualedNextTimePoint()
        {
            var preset = GetPreset(TimeSpan.Zero);
            var pvm = GetPresetViewModel(preset);
            var queue = GetQueue().GetTimerQueue(preset).ToArray();

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs(prevTimePoint: queue[0].nextTimePoint,
                                                                                            nextTimePoint: queue[1].nextTimePoint,
                                                                                            lastTimeToNextChange: default(TimeSpan),
                                                                                            prevTimePointNextBaseTime: null
                                                                                           ));

            Assert.AreEqual(queue[1].nextTimePoint, pvm.TimePointVmCollection.First(tp => tp.IsActive).TimePoint);
        }

        [Test]
        public void OnTimePointChangedEventHandler__StartTimePointIsPrev_ExistedTimePointIsNext__DoesntChangeStartTimePointBaseTime()
        {
            var preset = GetPreset(TimeSpan.Zero);
            var pvm = GetPresetViewModel(preset);
            var queue = GetQueue().GetTimerQueue(preset).ToArray();
            TimeSpan? expectedBaseTime = queue[0].nextTimePoint.BaseTime;

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs(prevTimePoint: queue[0].nextTimePoint,
                                                                                            nextTimePoint: queue[1].nextTimePoint,
                                                                                            lastTimeToNextChange: default(TimeSpan),
                                                                                            prevTimePointNextBaseTime: null
                                                                                           ));

            Assert.AreEqual(expectedBaseTime, queue[0].nextTimePoint.BaseTime);
        }

        [Test]
        public void OnTimePointChangedEventHandler__ExistedTimePointIsPrev_ExistedTimePointIsNext__ActivateTimePointViewModelEqualedNextTimePoint()
        {
            var preset = GetPreset(TimeSpan.Zero);
            var pvm = GetPresetViewModel(preset);
            var queue = GetQueue().GetTimerQueue(preset).ToArray();

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs(prevTimePoint: queue[1].nextTimePoint,
                                                                                            nextTimePoint: queue[2].nextTimePoint,
                                                                                            lastTimeToNextChange: default(TimeSpan),
                                                                                            prevTimePointNextBaseTime: null
                                                                                           ));

            Assert.AreEqual(queue[2].nextTimePoint, pvm.TimePointVmCollection.First(tp => tp.IsActive).TimePoint);
        }

        [Test]
        public void OnTimePointChangedEventHandler__ExistedTimePointIsPrev_ExistedTimePointIsNext__DeactivateTimePointViewModelEqualedPrevTimePoint()
        {
            var preset = GetPreset(TimeSpan.Zero);
            var pvm = GetPresetViewModel(preset);
            var queue = GetQueue().GetTimerQueue(preset).ToArray();
            TimeSpan? expectedBaseTime = queue[0].nextTimePoint.BaseTime;

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs(prevTimePoint: queue[0].nextTimePoint,
                                                                                            nextTimePoint: queue[1].nextTimePoint,
                                                                                            lastTimeToNextChange: default(TimeSpan),
                                                                                            prevTimePointNextBaseTime: null
                                                                                           ));

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs(prevTimePoint: queue[1].nextTimePoint,
                                                                                            nextTimePoint: queue[2].nextTimePoint,
                                                                                            lastTimeToNextChange: default(TimeSpan),
                                                                                            prevTimePointNextBaseTime: null
                                                                                           ));

            Assert.IsFalse(pvm.TimePointVmCollection.First(tp => tp.Equals(queue[1].nextTimePoint)).IsActive);
        }

        [Test]
        public void OnTimePointChangedEventHandler__ExistedTimePointIsPrev_ExistedTimePointIsNext__ChangesPrevTimePointBaseTime()
        {
            // Arrange
            var preset = GetPreset(TimeSpan.Zero);
            var pvm = GetPresetViewModel(preset);
            var queue = GetQueue().GetTimerQueue(preset).ToArray();
            TimeSpan dontExpectedAbsoluteTime = queue[1].nextTimePoint.GetAbsoluteTime();

            // Action:
            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs ( prevTimePoint: queue[0].nextTimePoint,
                                                                                              nextTimePoint: queue[1].nextTimePoint,
                                                                                              lastTimeToNextChange: default(TimeSpan),
                                                                                              prevTimePointNextBaseTime: null
                                                                                            ));

            _mockTimerManager.Raise(t => t.TimePointChanged += null, new TimerEventArgs ( prevTimePoint: queue[1].nextTimePoint,
                                                                                              nextTimePoint: queue[2].nextTimePoint,
                                                                                              lastTimeToNextChange: default(TimeSpan),
                                                                                              prevTimePointNextBaseTime: TimeSpan.FromHours (1)
                                                                                            ));

            // Assert
            Assert.AreNotEqual(dontExpectedAbsoluteTime, queue[1].nextTimePoint.GetAbsoluteTime());
        }


        #region Factory

        public string StartTimePointName { get; } = "StartTime";
        public TimePoint GetStartTimePoint(TimeSpan startTime)
        {
            return new TimePoint(StartTimePointName, startTime, TimePointType.Absolute);
        }


        private Mock<ITimerManager> _mockTimerManager;
        private PresetCollectionManager _presetCollectionManager;

        private Preset GetPreset(TimeSpan startTime) => new Preset(new []
                                                        {
                                                            new TimePoint("Test TimePoint # 1", "0:02:00", TimePointType.Relative, 0),
                                                            new TimePoint("Test TimePoint # 2", "0:02:00", TimePointType.Relative, 0),
                                                            new TimePoint("Test TimePoint # 3", "0:02:00", TimePointType.Relative, 1),
                                                            new TimePoint("Test TimePoint # 4", "0:02:00", TimePointType.Relative, 1),
                                                        }) { StartTime = startTime, TimerLoops = { [0] = 2, [1] = 2 } };

        private ITimerQueueCalculator GetQueue() => new TimerQueueCalculator(this);

        private TimeSpan[][] GetAbsoluteTimes(Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)> queue)
        {
            return null;
        }

        private PresetViewModel GetPresetViewModel(Preset preset)
        {
            _presetCollectionManager = new PresetCollectionManager();

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
