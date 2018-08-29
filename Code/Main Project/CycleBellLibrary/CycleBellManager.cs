using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (!PresetsManager.Presets.Any(p => p.PresetName.Equals(newPreset.PresetName)))
                _presetsManager.Add(Preset.EmptyPreset);
            else {
                throw new ArgumentException ("Can't create new empty preset. Empty preset already exists.");
            }
        }

        public void AddPreset(Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException (nameof(preset), "preset can't be null");

            if (_presetsManager.Presets.Any (p => p.PresetName == preset.PresetName)) 
                throw new ArgumentException("preset already exists", nameof(preset));

            _presetsManager.Add (preset);
        }

        public void DeletePreset (Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException (nameof(preset), "preset can't be null");

            if (!_presetsManager.Presets.Contains(preset))
                throw new ArgumentException("preset doesn't exists", nameof(preset));

            _presetsManager.Remove (preset);
        }
    }
}
