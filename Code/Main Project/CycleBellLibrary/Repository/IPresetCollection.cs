﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Context
{
    public interface IPresetCollection
    {
        ReadOnlyObservableCollection<Preset> Presets { get; }

        void Clear();
        void LoadFromFile(string fileName);
        void SavePresets(string fileName);
        void RenamePreset (Preset preset, string newName);
    }
}
