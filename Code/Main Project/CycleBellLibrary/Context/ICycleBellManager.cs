using System;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary.Context
{
    [Flags]
    public enum CantCreateNewPresetReasons
    {
        EmptyPresetNotModified = 0x0,
        EmptyPresetModified = 0x2,
        UnknownReason = 0x128
    }

    public class CantCreateNewPreetEventArgs : EventArgs
    {
        public CantCreateNewPreetEventArgs(CantCreateNewPresetReasons cantCreateNewPresetReason, Preset preset)
        {
            Preset = preset ?? throw new ArgumentNullException();
            CantCreateNewPresetReason = cantCreateNewPresetReason;
        }

        public CantCreateNewPresetReasons CantCreateNewPresetReason { get; }
        public Preset Preset { get; }
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

        void CheckCreateNewPreset(Preset existEmptyPreset);

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
