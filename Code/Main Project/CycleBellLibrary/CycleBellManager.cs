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
        private readonly IPresetsManager _presetsManager;
        private readonly ITimerManager _timerManager;

        public CycleBellManager (string fileName, IPresetsManager presetsManager, ITimerManager timerManager)
        {
            FileName = fileName;
            _presetsManager = presetsManager;
            _timerManager = timerManager;
        }

        public CycleBellManager (IPresetsManager presetsManager, ITimerManager timerManager)
            : this (null, presetsManager, timerManager)
        {}

        public string FileName { get; }
        public ICollection<Preset> Presets => _presetsManager.Presets;

        public event NotifyCollectionChangedEventHandler PresetCollectionChanged
        {
            add => ((INotifyCollectionChanged) Presets).CollectionChanged += value;
            remove => ((INotifyCollectionChanged) Presets).CollectionChanged -= value;
        }

        public void LoadPresetsFromFile (string fileName)
        {
            if (String.IsNullOrWhiteSpace (fileName))
                throw new ArgumentException("File name isn't correct");

            _presetsManager.LoadFromFile (fileName);
        }

        /// <summary>
        /// Creates empty preset
        /// </summary>
        /// <exception cref="ArgumentException">Throws when empty preset already exists</exception>
        public void CreateNewPreset()
        {
            var newPreset = Preset.EmptyPreset;

            if (!Presets.Any(p => p.PresetName.Equals(newPreset.PresetName)))
                _presetsManager.Add(Preset.EmptyPreset);
            else {
                throw new ArgumentException ("Can't create new empty preset. Empty preset already exists.");
            }
        }
    }
}
