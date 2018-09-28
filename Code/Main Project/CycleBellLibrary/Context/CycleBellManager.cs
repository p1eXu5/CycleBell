using System;
using System.Linq;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary.Context
{
    public class CycleBellManager : ICycleBellManager
    {
        #region Fields

        private readonly IInnerPresetCollectionManager _presetCollectionManager;

        #endregion

        #region Constructors

        public CycleBellManager (string fileName, IInnerPresetCollectionManager presetCollectionManager, ITimerManager timerManager)
        {
            FileName = fileName;
           _presetCollectionManager = presetCollectionManager ?? throw new ArgumentNullException(nameof(presetCollectionManager), "presetCollectionManager can't be null");
            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
        }

        public CycleBellManager (IInnerPresetCollectionManager presetCollectionManager, ITimerManager timerManager)
            : this (null, presetCollectionManager, timerManager)
        {}

        #endregion

        #region Properties

        public IPresetCollectionManager PresetCollectionManager => _presetCollectionManager;
        public ITimerManager TimerManager { get; }

        public string FileName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates empty preset when it doesn't exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when empty preset already exists and it is particulary filled</exception>
        public void CreateNewPreset()
        {
            var existEmptyPreset = PresetCollectionManager.Presets.FirstOrDefault (p => p.PresetName == Preset.DefaultName);

            if (existEmptyPreset == null) {
                _presetCollectionManager.Add (Preset.EmptyPreset);
            }
            else {
                // TODO if existing EmptyPreset does not equal Preset.EmptyPreset than throw, else - do nothing
                if (existEmptyPreset.StartTime != Preset.DefaultStartTime || existEmptyPreset.TimePointCollection.Count > 0) {
                    OnCantCreateNewPreset();
                }
            }
        }

        public event Action CantCreateNewPresetEvent;

        private void OnCantCreateNewPreset()
        {
            CantCreateNewPresetEvent?.Invoke();
        }

        public void OpenPresets (string fileName)
        {
            PresetCollectionManager.OpenPresets(fileName);
        }

        public void DeletePreset (Preset preset)
        {
            _presetCollectionManager.Remove (preset);
        }

        /// <summary>
        /// Adds preset
        /// </summary>
        /// <param name="preset"></param>
        public void AddPreset(Preset preset)
        {
            _presetCollectionManager.Add (preset);
        }

        /// <summary>
        /// Remove preset
        /// </summary>
        /// <param name="preset"></param>
        public void RemovePreset (Preset preset)
        {
            _presetCollectionManager.Remove (preset);
        }

        /// <summary>
        /// Serialize presets
        /// </summary>
        public void SavePresets() => PresetCollectionManager.SavePresets(FileName);

        public void SavePresets (string fileName)
        {
            PresetCollectionManager.SavePresets (fileName);
        }

        public void RenamePreset (Preset preset, string newName)
        {
            if (preset == null || !_presetCollectionManager.Contains (preset) || newName == null)
                return;

            var presetWithSameNewName = _presetCollectionManager.FirstOrDefault (p => p.PresetName == newName);

            if (presetWithSameNewName == null)
                preset.PresetName = newName;
        }

        #endregion
    }
}
