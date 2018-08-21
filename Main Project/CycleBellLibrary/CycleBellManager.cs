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
    public class CycleBellManager
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
            : this ("", presetsManager, timerManager)
        {}

        public string FileName { get; }
        public ReadOnlyObservableCollection<Preset> Presets => _presetsManager.Presets;

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => ((INotifyCollectionChanged) Presets).CollectionChanged += value;
            remove => ((INotifyCollectionChanged) Presets).CollectionChanged -= value;
        }

        public void LoadPresetsFromFile (string fileName) => _presetsManager.LoadFromFile (fileName);
    }
}
