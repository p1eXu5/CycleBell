using System;
using System.Linq;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary
{
    public class CycleBellManager : ICycleBellManager
    {
        #region Fields

        private readonly IInnerPresetsManager _presetsManager;

        #endregion

        #region Constructors

        public CycleBellManager (string fileName, IInnerPresetsManager presetsManager, ITimerManager timerManager)
        {
            FileName = fileName;
           _presetsManager = presetsManager ?? throw new ArgumentNullException(nameof(presetsManager), "presetsManager can't be null");
            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
        }

        public CycleBellManager (IInnerPresetsManager presetsManager, ITimerManager timerManager)
            : this (null, presetsManager, timerManager)
        {}

        #endregion

        #region Properties

        public IPresetsManager PresetsManager => _presetsManager;
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
            var existEmptyPreset = PresetsManager.Presets.FirstOrDefault (p => p.PresetName == Preset.DefaultName);

            if (existEmptyPreset == null) {
                _presetsManager.Add (Preset.EmptyPreset);
            }
            else {
                // TODO if existing EmptyPreset does not equal Preset.EmptyPreset than throw, else - do nothing
                if (existEmptyPreset.StartTime != Preset.DefaultStartTime || existEmptyPreset.TimePoints.Count > 0) {
                    throw new InvalidOperationException ("Can't create new empty preset. Empty preset already exists and it is particulary filled.");
                }
            }
        }

        public void DeletePreset (Preset preset)
        {
            _presetsManager.Remove (preset);
        }

        /// <summary>
        /// Adds preset
        /// </summary>
        /// <param name="preset"></param>
        public void AddPreset(Preset preset)
        {
            _presetsManager.Add (preset);
        }

        /// <summary>
        /// Remove preset
        /// </summary>
        /// <param name="preset"></param>
        public void RemovePreset (Preset preset)
        {
            _presetsManager.Remove (preset);
        }

        /// <summary>
        /// Serialize presets
        /// </summary>
        public void SavePresets() => PresetsManager.SavePresets(FileName);

        public void RenamePreset (Preset preset, string newName)
        {
            if (preset == null || !_presetsManager.Contains (preset) || newName == null)
                return;

            var presetWithSameNewName = _presetsManager.FirstOrDefault (p => p.PresetName == newName);

            if (presetWithSameNewName == null)
                preset.PresetName = newName;
        }

        #endregion
    }
}
