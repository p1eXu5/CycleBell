using System;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary.Context
{
    public interface ICycleBellManager
    {
        IPresetCollectionManager PresetCollectionManager { get; }
        ITimerManager TimerManager { get; }

        string FileName { get; }

        /// <summary>
        /// Creates a new preset
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void CreateNewPreset();

        void OpenPresets (string fileName);

        void DeletePreset (Preset preset);

        void RenamePreset (Preset preset, string newName);

        /// <summary>
        /// Serializes into xml file
        /// </summary>
        void SavePresets();

        void SavePresets (string fileName);

        event Action CantCreateNewPresetEvent;
    }
}
