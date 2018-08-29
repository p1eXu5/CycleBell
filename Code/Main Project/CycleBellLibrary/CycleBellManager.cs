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
        #region ctor

        public CycleBellManager (string fileName, IInnerPresetsManager presetsManager, ITimerManager timerManager)
        {
            FileName = fileName;
            PresetsManager = (IPresetsManager)presetsManager;
            TimerManager = timerManager;
        }

        public CycleBellManager (IInnerPresetsManager presetsManager, ITimerManager timerManager)
            : this (null, presetsManager, timerManager)
        {}

        #endregion

        public IPresetsManager PresetsManager { get; }
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
                ((IInnerPresetsManager)PresetsManager).Add(Preset.EmptyPreset);
            else {
                throw new ArgumentException ("Can't create new empty preset. Empty preset already exists.");
            }
        }
    }
}
