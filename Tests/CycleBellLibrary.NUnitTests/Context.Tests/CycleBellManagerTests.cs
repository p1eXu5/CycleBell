﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests
{
    [TestFixture]
    public class CycleBellManagerTests
    {
        #region Fields

        private readonly PresetCollection _presetCollection = new PresetCollection();
        private readonly Mock<IInnerPresetCollection> _mockPresetsManager = new Mock<IInnerPresetCollection>();

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

            Assert.IsTrue(cbm.PresetCollection.Presets.Count == 1);
        }

        [Test]
        public void CreateNewPreset_EmptyPresetExists_DoesNotCallPresetManager()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();
            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetCollection.Presets.Count == 1);
        }

        [Test]
        public void AddPreset_WhenCalled_CallsPresetManager()
        {
            var cbm = GetCycleBellManager();
            var addingPreset = Preset.EmptyPreset;
            
            cbm.AddPreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollection.Presets.Count == 1);
        }

        [Test]
        public void RemovePreset_WhenCalled_CallsPresetsManager()
        {
            var cbm = GetCycleBellManager();
            var addingPreset = Preset.EmptyPreset;
            
            cbm.AddPreset (addingPreset);

            cbm.RemovePreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollection.Presets.Count == 0);
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
            public event NotifyCollectionChangedEventHandler PresetCollectionChanged;
            public event EventHandler<TimerEventArgs> ChangeTimePointEvent;
            public event EventHandler<TimerEventArgs> TimerSecondPassedEvent;
            public event EventHandler TimerStopEvent;
            public ReadOnlyObservableCollection<Preset> Presets { get; }
            public bool IsRunning { get; }
            public void AddPreset (Preset preset)
            {
                throw new NotImplementedException();
            }

            public void Pouse()
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

            public Queue<(TimeSpan, TimePoint)> GetTimerQueue(Preset preset)
            {
                throw new NotImplementedException();
            }

            public int GetIndex (string name)
            {
                throw new NotImplementedException();
            }
        }
        #pragma warning restore 0067

        #endregion
    }
}