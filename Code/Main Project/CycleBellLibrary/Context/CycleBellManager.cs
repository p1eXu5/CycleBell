﻿/*
 * Copyright © 2018 Vladimir Likhatskiy. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *  
 */

using System;
using System.IO;
using System.Linq;
using CycleBellLibrary.Models;
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
            catch(FileNotFoundException) { }
        }

        public CycleBellManager (IInnerPresetCollectionManager presetCollectionManager, ITimerManager timerManager)
            : this (null, presetCollectionManager, timerManager)
        {}

        #endregion

        #region Properties

        public IPresetCollectionManager PresetCollectionManager => _presetCollectionManager;
        public ITimerManager TimerManager { get; }

        public string FileName { get; }

        public bool IsEmptyPresetExists =>
            PresetCollectionManager.Presets.FirstOrDefault(p => p.PresetName == Preset.DefaultName) != null;

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
                OnCantCreateNewPreset(CantCreateNewPresetReasonsFlags.EmptyPresetModified, existEmptyPreset);
            }
            else {
                OnCantCreateNewPreset(CantCreateNewPresetReasonsFlags.EmptyPresetNotModified, existEmptyPreset);
            }
        }

        public event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;

        private void OnCantCreateNewPreset(CantCreateNewPresetReasonsFlags reasonFlags, Preset preset)
        {
            CantCreateNewPresetEvent?.Invoke(this, new CantCreateNewPreetEventArgs(reasonFlags, preset));
        }

        public void OpenPresets() => OpenPresets(FileName);
        public void OpenPresets (string fileName)
        {
            PresetCollectionManager.OpenPresets(fileName);
        }

        public void ClearPresets()
        {
            PresetCollectionManager.Clear();
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
