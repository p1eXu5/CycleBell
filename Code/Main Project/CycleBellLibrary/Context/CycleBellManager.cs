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

        private readonly IInnerPresetCollection _presetCollection;

        #endregion

        #region Constructors

        public CycleBellManager (string fileName, IInnerPresetCollection presetCollection, ITimerManager timerManager)
        {
            FileName = fileName;
           _presetCollection = presetCollection ?? throw new ArgumentNullException(nameof(presetCollection), "presetCollection can't be null");
            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
        }

        public CycleBellManager (IInnerPresetCollection presetCollection, ITimerManager timerManager)
            : this (null, presetCollection, timerManager)
        {}

        #endregion

        #region Properties

        public IPresetCollection PresetCollection => _presetCollection;
        public ITimerManager TimerManager { get; }

        public string FileName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates empty preset when it doesn't exist.
        /// </summary>
        /// <exception cref="ArgumentException">Throws when empty preset already exists and it is particulary filled</exception>
        public void CreateNewPreset()
        {
            var existEmptyPreset = PresetCollection.Presets.FirstOrDefault (p => p.PresetName == Preset.DefaultName);

            if (existEmptyPreset == null) {
                _presetCollection.Add (Preset.EmptyPreset);
            }
            else {
                // TODO if existing EmptyPreset does not equal Preset.EmptyPreset than throw, else - do nothing
                if (existEmptyPreset.StartTime != Preset.DefaultStartTime || existEmptyPreset.TimePoints.Count > 0) {
                    throw new ArgumentException ("Can't create new empty preset. Empty preset already exists and it is particulary filled.");
                }
            }
        }

        /// <summary>
        /// Adds preset
        /// </summary>
        /// <param name="preset"></param>
        public void AddPreset(Preset preset)
        {
            _presetCollection.Add (preset);
        }

        /// <summary>
        /// Remove preset
        /// </summary>
        /// <param name="preset"></param>
        public void RemovePreset (Preset preset)
        {
            _presetCollection.Remove (preset);
        }

        /// <summary>
        /// Serialize presets
        /// </summary>
        public void SavePresets() => PresetCollection.SavePresets(FileName);

        public void RenamePreset (Preset preset, string newName)
        {
            if (preset == null || !_presetCollection.Contains (preset) || newName == null)
                return;

            var presetWithSameNewName = _presetCollection.FirstOrDefault (p => p.PresetName == newName);

            if (presetWithSameNewName == null)
                preset.PresetName = newName;
        }

        #endregion
    }
}
