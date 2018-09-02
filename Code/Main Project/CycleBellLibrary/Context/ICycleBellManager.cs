using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary
{
    public interface ICycleBellManager
    {
        IPresetCollectionWrap PresetCollectionWrap { get; }
        ITimerManager TimerManager { get; }
        string FileName { get; }

        /// <summary>
        /// Creates a new preset
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void CreateNewPreset();

        void SavePresets();
        void RenamePreset (Preset preset, string newName);
    }
}
