using System;
using System.Text;
using System.Collections.Generic;
using CycleBellLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.ObjectModel;

namespace CycleBellLibraryTests
{
    /// <summary>
    /// Summary description for CycleBellTimerManagerTests
    /// </summary>
    [TestClass]
    public class CycleBellTimerManagerTests
    {
        private readonly Mock<IPresetsManager> mock = new Mock<IPresetsManager>();
        private ObservableCollection<Preset> _presets;
        private ReadOnlyObservableCollection<Preset> _readOnlePresets;
        private TimeSpan[][] checker;

        [TestInitialize]
        public void TestInitialize()
        {
            TimePoint[] points =
            {
                new TimePoint(new TimeSpan(1, 0, 0)),
                new TimePoint(new TimeSpan(20, 0, 0), TimePointType.Absolute, timerCycleNum: 3),
                new TimePoint(new TimeSpan(2, 0, 0), timerCycleNum: 1),
                new TimePoint(new TimeSpan(1, 0, 0), timerCycleNum: 2),
            };

            TimePoint[] points2 =
            {
                new TimePoint(new TimeSpan(10, 0, 0), TimePointType.Absolute),
                new TimePoint(new TimeSpan(20, 0, 0), TimePointType.Absolute, timerCycleNum: 3),
                new TimePoint(new TimeSpan(2, 0, 0), timerCycleNum: 1),
                new TimePoint(new TimeSpan(1, 0, 0), timerCycleNum: 2),
            };


            _presets = new ObservableCollection<Preset>(new[]
            {
                new Preset(points) {StartTime = new TimeSpan(12, 0, 0)},
                new Preset(points2) {StartTime = new TimeSpan(12, 0, 0)}
            });

            _readOnlePresets = new ReadOnlyObservableCollection<Preset>(_presets);

            mock.SetupGet(m => m.Presets).Returns(_readOnlePresets);

            checker = new []
            {
                new[] 
                {
                    new TimeSpan(12, 0, 0),
                    new TimeSpan(13, 0, 0),
                    new TimeSpan(15, 0, 0),
                    new TimeSpan(16, 0, 0),
                    new TimeSpan(20, 0, 0)

                },
                new[] 
                {
                    new TimeSpan(12, 0, 0),
                    new TimeSpan(13, 0, 0),
                    new TimeSpan(15, 0, 0),
                    new TimeSpan(16, 0, 0),
                    new TimeSpan(20, 0, 0)
                }
            };
        }

        [TestMethod]
        public void GetTimerQueueReturnsOrderedQueue()
        {
            var manager = TimerManager.Instance(mock.Object);
            var queue = manager.GetTimerQueue(manager.Presets[0]);
            var queue2 = manager.GetTimerQueue(manager.Presets[1]);

            var indx = 0;

            foreach (var point in queue) {
                Assert.AreEqual(point.Item1, checker[0][indx++]);
            }

            indx ^= indx;

            foreach (var point in queue2) {
                Assert.AreEqual(point.Item1, checker[1][indx++]);
            }
        }

        [TestMethod]
        public void CycleBellTimerManagerReturnsEmptyPreset()
        {
            PresetsManager presetsManager = new PresetsManager();
            var manager = TimerManager.Instance(mock.Object);

            Assert.AreEqual(manager.Presets[0].PresetName, Preset.EmptyPreset.PresetName);
            Assert.AreEqual(manager.Presets.Count, _presets.Count);
        }
    }
}
