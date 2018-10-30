using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Context.Tests
{
    [TestFixture]
    public class CycleBellManagerTests
    {
        #region Fields

        private readonly Mock<IInnerPresetCollectionManager> _mockPresetsManager = new Mock<IInnerPresetCollectionManager>();

        #endregion

        [SetUp]
        public void TestInitializer()
        {
            List<Preset> presetList = new List<Preset>();
            _mockPresetsManager.Setup (m => m.Add (It.IsAny<Preset>())).Callback ((Preset p) => presetList.Add (p));
            _mockPresetsManager.Setup (m => m.Presets).Returns (() => new ReadOnlyObservableCollection<Preset>(new ObservableCollection<Preset>(presetList)));
            _mockPresetsManager.Setup (m => m.Remove (It.IsAny<Preset>())).Callback ((Preset p) => presetList.Remove (p));
        }

        [Test]
        public void CreateNewPreset_EmptyPresetDoesntExist_CallsPresetManager()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetCollectionManager.Presets.Count == 1);
        }

        [Test]
        public void CreateNewPreset_DoubleCallEmptyPresetExists_DoesNotCallPresetManager()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();
            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetCollectionManager.Presets.Count == 1);
        }

        [Test]
        public void AddPreset_WhenCalled_CallsPresetManager()
        {
            var cbm = GetCycleBellManager();
            var addingPreset = Preset.GetDefaultPreset();
            
            cbm.AddPreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollectionManager.Presets.Count == 1);
        }

        [Test]
        public void RemovePreset_WhenCalled_CallsPresetsManager()
        {
            var cbm = GetCycleBellManager();
            var addingPreset = Preset.GetDefaultPreset();
            
            cbm.AddPreset (addingPreset);

            cbm.RemovePreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollectionManager.Presets.Count == 0);
        }

        [Test]
        public void IsNewPreset_DefaulPreset_ReturnsTrue()
        {
            var preset = new Preset();
            var cbm = GetCycleBellManager();

            Assert.That(cbm.IsNewPreset(preset), Is.EqualTo(true));
        }

        [Test]
        public void IsNewPreset_NamedPreset_ReturnsFalse()
        {
            var preset = new Preset("asd");
            var cbm = GetCycleBellManager();

            Assert.That(cbm.IsNewPreset(preset), Is.EqualTo(false));
        }

        #region Factory

        private CycleBellManager GetCycleBellManager()
        {
            FakeTimerManager stubTimerManager = new FakeTimerManager();

            return new CycleBellManager (_mockPresetsManager.Object, stubTimerManager);
        }

        #endregion

        #region FakeTypes

        #pragma warning disable 0067
        internal class FakeTimerManager : ITimerManager
        {
            public event EventHandler<TimerEventArgs> ChangeTimePointEvent;
            public event EventHandler<TimerEventArgs> TimerSecondPassedEvent;
            public event EventHandler TimerStartEvent;
            public event EventHandler TimerPauseEvent;
            public event EventHandler TimerStopEvent;
            public bool IsRunning { get; } = false;
            public bool IsPaused { get; } = false;
            public string StartTimePointName { get; } = null;

            public void Pause()
            {
                throw new NotImplementedException();
            }

            public void Resume()
            {
                throw new NotImplementedException();
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }

            public void PlayAsync (Preset preset)
            {
                throw new NotImplementedException();
            }

            public void Play (Preset preset)
            {
                throw new NotImplementedException();
            }

            public void DontPreserveBaseTime()
            {}

            public void PreserveBaseTime()
            {}
        }
        #pragma warning restore 0067

        #endregion
    }
}
