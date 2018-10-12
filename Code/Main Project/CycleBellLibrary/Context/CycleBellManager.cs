﻿using System;
using System.IO;
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

            try {
                OpenPresets();
            }
            catch(FileNotFoundException ex) { }
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
        public bool CreateNewPreset()
        {
            var existEmptyPreset = PresetCollectionManager.Presets.FirstOrDefault (p => p.PresetName == Preset.DefaultName);
            bool res;

            if (existEmptyPreset == null) {
                _presetCollectionManager.Add (Preset.EmptyPreset);
                res = true;
            }
            else {
                CheckCreateNewPreset(existEmptyPreset);

                res = false;
            }

            return res;
        }

        public void CheckCreateNewPreset(Preset existEmptyPreset)
        {
            if (existEmptyPreset.StartTime != Preset.DefaultStartTime || existEmptyPreset.TimePointCollection.Count > 0 || existEmptyPreset.StartTime != Preset.DefaultStartTime) {
                OnCantCreateNewPreset(CantCreateNewPresetReasons.EmptyPresetModified, existEmptyPreset);
            }
            else {
                OnCantCreateNewPreset(CantCreateNewPresetReasons.EmptyPresetNotModified, existEmptyPreset);
            }
        }

        public event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;

        private void OnCantCreateNewPreset(CantCreateNewPresetReasons reason, Preset preset)
        {
            CantCreateNewPresetEvent?.Invoke(this, new CantCreateNewPreetEventArgs(reason, preset));
        }

        public void OpenPresets() => OpenPresets(FileName);
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
