using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests
{
    [TestFixture]
    public class CycleBellManagerTests
    {
        #region Fields

        private readonly PresetsManager _presetsManager = new PresetsManager();

        #endregion

        [Test]
        public void CreateNewPreset_EmptyPresetDoesntExist_CallsPresetManager()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetsManager.Presets.Count == 1);
        }

        [Test]
        public void CreateNewPreset_EmptyPresetExists_DoesNotCallPresetManager()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();
            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetsManager.Presets.Count == 1);
        }

        #region Factory

        private CycleBellManager GetCycleBellManager()
        {
            FakeTimerManager stubTimerManager = new FakeTimerManager();

            return new CycleBellManager (_presetsManager, stubTimerManager);
        }

        #endregion

        #region FakeTypes

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
