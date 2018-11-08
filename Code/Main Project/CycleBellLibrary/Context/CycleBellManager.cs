/*
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
            TimerManager.DontPreserveBaseTime();

           _presetCollectionManager = presetCollectionManager ?? throw new ArgumentNullException(nameof(presetCollectionManager), "presetCollectionManager can't be null");
           PresetCollectionManager.Clear();

            try {
                OpenPresets();
                RemoveNewPresets();
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


        #endregion

        #region Methods

        /// <summary>
        /// Creates and adds a new preset when equaled a new preset doesn't
        /// exist in <see cref="Repository.PresetCollectionManager.Presets"/> collection.
        /// </summary>
        /// <returns><see cref="Boolean"/> if preset added</returns>
        public bool CreateNewPreset()
        {
            bool res;
            var existingNewPreset = FindNewPreset();

            // if new Preset does not exist
            if (existingNewPreset == null) {

                _presetCollectionManager.Add (GetNewPreset());
                res = true;
            }
            else {
                RiseCantCreateNewPreset (existingNewPreset);
                res = false;
            }

            return res;
        }

        /// <summary>
        /// Searches the new Preset. Returns null if new Preset does not exist.
        /// </summary>
        /// <returns></returns>
        private Preset FindNewPreset()
        {
            return PresetCollectionManager.Presets.FirstOrDefault (IsNewPreset);
        }

        /// <summary>
        /// Gets instance of a new Preset
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Preset GetNewPreset() => Preset.GetDefaultPreset();

        public Preset[] GetNewPresets() =>
            PresetCollectionManager.Presets.Where(IsNewPreset).ToArray();

        public void RiseCantCreateNewPreset(Preset existingNewPreset)
        {
            if (PresetChecker.IsModifiedPreset (existingNewPreset)) {
                OnCantCreateNewPreset(CantCreateNewPresetReasonsEnum.NewPresetModified, existingNewPreset);
            }
            else {
                OnCantCreateNewPreset(CantCreateNewPresetReasonsEnum.NewPresetNotModified, existingNewPreset);
            }

        }

        public bool IsNewPreset(Preset preset) => PresetChecker.IsNewPreset(preset);

        public event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;

        private void OnCantCreateNewPreset(CantCreateNewPresetReasonsEnum reasonEnum, Preset preset)
        {
            CantCreateNewPresetEvent?.Invoke(this, new CantCreateNewPreetEventArgs(reasonEnum, preset));
        }

        public void OpenPresets() => OpenPresets(FileName);
        public void OpenPresets (string fileName)
        {
            PresetCollectionManager.OpenPresets(fileName);
        }

        public void RemoveNewPresets()
        {
            var newPresets = GetNewPresets();

            if (newPresets.Length > 0) {
                RemovePresets(newPresets);
            }
        }

        /// <summary>
        /// Remove preset
        /// </summary>
        /// <param name="preset"></param>
        public void RemovePreset (Preset preset)
        {
            _presetCollectionManager.Remove (preset);
        }

        public void RemovePresets(Preset[] presets)
        {
            if (presets.Any()) {

                foreach (var preset in presets) {

                    PresetCollectionManager.Remove(preset);
                }
            }
        }

        public void ClearPresets()
        {
            PresetCollectionManager.Clear();
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
