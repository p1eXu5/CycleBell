﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests
{
    [TestFixture]
    public class CycleBellManagerTests
    {
        [Test]
        public void CreateNewPreset_PresetsDoesntContainsEmptyPreset_AddsEmptyPreset()
        {
            CycleBellManager cycleBellManager = GetCycleBellManager();

            cycleBellManager.CreateNewPreset();

            Assert.IsTrue(cycleBellManager.PresetsManager.Presets.Count == 1);
        }

        [Test]
        public void CreateNewPreset_PresetsContainsEmptyPreset_Throws()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();
            var ex = Assert.Catch<ArgumentException> (() => cbm.CreateNewPreset());

            StringAssert.Contains ("Can't create new empty preset. Empty preset already exists.", ex.Message);
        }



        [Test]
        public void DeletePreset_ExistingPreset_DeletesPreset()
        {
            var cbm = GetCycleBellManager();
            cbm.CreateNewPreset();
            var addedPreset = cbm.PresetsManager.Presets[0];

            cbm.DeletePreset (addedPreset);

            Assert.IsTrue (cbm.PresetsManager.Presets.Count == 0);
        }

        #region Factory

        private CycleBellManager GetCycleBellManager()
        {
            FakePresetManager stubPresetManager = new FakePresetManager();
            FakeTimerManager stubTimerManager = new FakeTimerManager();

            return new CycleBellManager (stubPresetManager, stubTimerManager);
        }

        #endregion

        #region FakeTypes

        internal class FakePresetManager : IInnerPresetsManager
        {
            private readonly ObservableCollection<Preset> _presets = new ObservableCollection<Preset>();

            public FakePresetManager()
            {
                Presets = new System.Collections.ObjectModel.ReadOnlyObservableCollection<Preset> (_presets);
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public string FileName { get; set; }
            public ReadOnlyObservableCollection<Preset> Presets { get; } = null;
            public void Clear()
            {
                throw new NotImplementedException();
            }

            public void LoadFromFile (string fileName)
            {
                throw new NotImplementedException();
            }

            public void Add (Preset preset)
            {
                _presets.Add(preset);
            }

            public void SavePresets()
            {
                throw new NotImplementedException();
            }
        }

        internal class FakeTimerManager : ITimerManager
        {
            public event NotifyCollectionChangedEventHandler PresetCollectionChanged;
            public event EventHandler<TimePointEventArgs> ChangeTimePointEvent;
            public event EventHandler<TimePointEventArgs> TimerSecondPassedEvent;
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

            public int GetIndex (string name)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}