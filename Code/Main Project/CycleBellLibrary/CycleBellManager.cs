using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary
{
    public class CycleBellManager : ICycleBellManager
    {
        private readonly IInnerPresetsManager _presetsManager;

        #region ctor

        public CycleBellManager (string fileName, IInnerPresetsManager presetsManager, ITimerManager timerManager)
        {
            FileName = fileName;
           _presetsManager = presetsManager ?? throw new ArgumentNullException(nameof(presetsManager), "presetsManager can't be null");
            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
        }

        public CycleBellManager (IInnerPresetsManager presetsManager, ITimerManager timerManager)
            : this (null, presetsManager, timerManager)
        {}

        #endregion

        public IPresetsManager PresetsManager => _presetsManager;
        public ITimerManager TimerManager { get; }

        public string FileName { get; }

        /// <summary>
        /// Creates empty preset
        /// </summary>
        /// <exception cref="ArgumentException">Throws when empty preset already exists</exception>
        public void CreateNewPreset()
        {
            var newPreset = Preset.EmptyPreset;

            if (PresetsManager.Presets != null && PresetsManager.Presets.All (p => p.PresetName != newPreset.PresetName))
                _presetsManager.Add(Preset.EmptyPreset);
            else {
                throw new ArgumentException ("Can't create new empty preset. Empty preset already exists.");
            }
        }

        public void AddPreset(Preset preset)
        {


            _presetsManager.Add (preset);
        }

        public void RemovePreset (Preset preset)
        {
           

            _presetsManager.Remove (preset);
        }
    }
}
