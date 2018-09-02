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

        private readonly IInnerPresetCollectionWrap _presetCollectionWrap;

        #endregion

        #region Constructors

        public CycleBellManager (string fileName, IInnerPresetCollectionWrap presetCollectionWrap, ITimerManager timerManager)
        {
            FileName = fileName;
           _presetCollectionWrap = presetCollectionWrap ?? throw new ArgumentNullException(nameof(presetCollectionWrap), "presetCollectionWrap can't be null");
            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
        }

        public CycleBellManager (IInnerPresetCollectionWrap presetCollectionWrap, ITimerManager timerManager)
            : this (null, presetCollectionWrap, timerManager)
        {}

        #endregion

        #region Properties

        public IPresetCollectionWrap PresetCollectionWrap => _presetCollectionWrap;
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
            var existEmptyPreset = PresetCollectionWrap.Presets.FirstOrDefault (p => p.PresetName == Preset.DefaultName);

            if (existEmptyPreset == null) {
                _presetCollectionWrap.Add (Preset.EmptyPreset);
            }
            else {
                // TODO if existing EmptyPreset does not equal Preset.EmptyPreset than throw, else - do nothing
                if (existEmptyPreset.StartTime != Preset.DefaultStartTime || existEmptyPreset.TimePoints.Count > 0) {
                    throw new InvalidOperationException ("Can't create new empty preset. Empty preset already exists and it is particulary filled.");
                }
            }
        }

        /// <summary>
        /// Adds preset
        /// </summary>
        /// <param name="preset"></param>
        public void AddPreset(Preset preset)
        {
            _presetCollectionWrap.Add (preset);
        }

        /// <summary>
        /// Remove preset
        /// </summary>
        /// <param name="preset"></param>
        public void RemovePreset (Preset preset)
        {
            _presetCollectionWrap.Remove (preset);
        }

        /// <summary>
        /// Serialize presets
        /// </summary>
        public void SavePresets() => PresetCollectionWrap.SavePresets(FileName);

        public void RenamePreset (Preset preset, string newName)
        {
            if (preset == null || !_presetCollectionWrap.Contains (preset) || newName == null)
                return;

            var presetWithSameNewName = _presetCollectionWrap.FirstOrDefault (p => p.PresetName == newName);

            if (presetWithSameNewName == null)
                preset.PresetName = newName;
        }

        #endregion
    }
}
