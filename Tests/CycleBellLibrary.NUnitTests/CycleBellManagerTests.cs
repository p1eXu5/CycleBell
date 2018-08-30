using System;
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
        #region Fields

        private readonly FakePresetsManager _mockPresetsManager = new FakePresetsManager();

        #endregion

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
        public void DeletePreset_WhenCalled_CallsPresetsManager()
        {
            var cbm = GetCycleBellManager();
            var preset = Preset.EmptyPreset;

            cbm.RemovePreset (preset);

            Assert.AreSame (_mockPresetsManager.RemovingPreset, preset);
        }

        #region Factory

        private CycleBellManager GetCycleBellManager()
        {
            FakeTimerManager stubTimerManager = new FakeTimerManager();

            return new CycleBellManager (_mockPresetsManager, stubTimerManager);
        }

        #endregion

        #region FakeTypes

        internal class FakePresetsManager : IInnerPresetsManager
        {
            public Preset AddingPreset { get; set; }
            public Preset RemovingPreset { get; set; }

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
                AddingPreset = preset;
            }

            public void Remove (Preset preset)
            {
                RemovingPreset = preset;
            }

            public void SavePresets()
            {
                throw new NotImplementedException();
            }
        }

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

            public int GetIndex (string name)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
