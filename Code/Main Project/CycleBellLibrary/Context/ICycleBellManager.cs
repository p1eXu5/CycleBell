using System;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary.Context
{
    [Flags]
    public enum CantCreateNewPresetReasons
    {
        EmptyPresetAlreadyExists = 0x1,
        EmptyPresetModified = 0x2,
        UnknownReason = 0x128
    }

    public class CantCreateNewPreetEventArgs : EventArgs
    {
        public CantCreateNewPreetEventArgs(CantCreateNewPresetReasons cantCreateNewPresetReason)
        {
            CantCreateNewPresetReason = cantCreateNewPresetReason;
        }

        public CantCreateNewPresetReasons CantCreateNewPresetReason { get; }
    }

    public interface ICycleBellManager
    {
        IPresetCollectionManager PresetCollectionManager { get; }
        ITimerManager TimerManager { get; }

        string FileName { get; }

        /// <summary>
        /// Creates a new preset
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        bool CreateNewPreset();

        void OpenPresets (string fileName);

        void DeletePreset (Preset preset);

        void RenamePreset (Preset preset, string newName);

        /// <summary>
        /// Serializes into xml file
        /// </summary>
        void SavePresets();

        void SavePresets (string fileName);

        event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;
    }
}
