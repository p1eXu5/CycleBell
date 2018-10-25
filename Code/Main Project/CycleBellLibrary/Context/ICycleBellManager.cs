using System;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary.Context
{
    public interface ICycleBellManager
    {
        IPresetCollectionManager PresetCollectionManager { get; }
        ITimerManager TimerManager { get; }

        /// <summary>
        /// Creates a new preset
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        bool CreateNewPreset();

        void CheckCreateNewPreset(Preset existEmptyPreset);

        void OpenPresets (string fileName);

        void DeletePreset (Preset preset);

        void ClearPresets();

        void RenamePreset (Preset preset, string newName);

        void SavePresets (string fileName);

        event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;
    }
}
