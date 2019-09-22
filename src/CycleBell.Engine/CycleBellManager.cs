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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CycleBell.Engine.Models;
using CycleBell.Engine.Models.Extensions;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;

namespace CycleBell.Engine
{
    public class CycleBellManager : ICycleBellManager
    {

        #region Constructors

        public CycleBellManager (string fileName, IPresetCollection presetCollection, ITimerManager timerManager)
        {
            FileName = fileName;

            TimerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager), "timerManager can't be null");
            TimerManager.DontPreserveBaseTime();

           PresetCollection = presetCollection ?? throw new ArgumentNullException(nameof(presetCollection), "presetCollection can't be null");
           PresetCollection.Clear();

            try {
                OpenPresets();
                RemoveDefaultPresets();
            }
            catch( FileNotFoundException ) { }

        }

        public CycleBellManager (IPresetCollection presetCollection, ITimerManager timerManager)
            : this (null, presetCollection, timerManager)
        {}

        #endregion


        #region events

        public event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent; 
        
        #endregion


        #region Properties

        public IPresetCollection PresetCollection { get; }
        public ITimerManager TimerManager { get; }
        public string FileName { get; }

        #endregion


        #region public

        /// <summary>
        /// Creates and adds a new existingPreset when equaled a new existingPreset doesn't
        /// exist in <see cref="Repository.PresetCollection"/> presetCollection.
        /// </summary>
        /// <returns><see cref="Boolean"/> if existingPreset added</returns>
        public bool CreateNewPreset()
        {
            bool res;
            var firstDefaultNamedPreset = FindFirstDefaultNamedPreset();

            // if new Preset does not exist
            if (firstDefaultNamedPreset == null) {

                PresetCollection.Add (Preset.GetDefaultPreset() );
                res = true;
            }
            else {
                RiseCantCreateNewPreset( firstDefaultNamedPreset );
                res = false;
            }

            return res;
        }

        /// <summary>
        /// Adds existingPreset
        /// </summary>
        /// <param name="preset"></param>
        public bool AddPreset(Preset preset)
        {
            if ( preset != null && !PresetCollection.Contains( preset ) ) {
                PresetCollection.Add( preset );
                return true;
            }

            return false;
        }

        public void ClearPresets()
        {
            PresetCollection.Clear();
        }

        public void RenamePreset (Preset preset, string newName)
        {
            if (preset == null || !PresetCollection.Contains (preset) || newName == null)
                return;

            var presetWithSameNewName = PresetCollection.FirstOrDefault (p => p.PresetName == newName);

            if (presetWithSameNewName == null)
                preset.PresetName = newName;
        }

        public void RemovePreset (Preset preset)
        {
            PresetCollection.Remove (preset);
        }

        public bool IsNewPreset( Preset preset ) => preset.IsDefaultNamed();

        public void OpenPresets (string fileName)
        {
            PresetCollection.Open(fileName);
        }

        public void OpenPresets() => OpenPresets(FileName);

        public void SavePresets (string fileName)
        {
            PresetCollection.Save (fileName);
        }

        /// <summary>
        /// Serialize presets to file
        /// </summary>
        public void SavePresets() => PresetCollection.Save( FileName );

        #endregion


        #region privates

        /// <summary>
        /// Searches the new Preset. Returns null if new Preset does not exist.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Preset FindFirstDefaultNamedPreset()
        {
            return PresetCollection.Presets.FirstOrDefault( p => p.IsDefaultNamed() );
        }

        private Preset[] GetDefaultNamedPresets() =>
            PresetCollection.Presets.Where( p => p.IsDefaultNamed() ).ToArray();

        private void RiseCantCreateNewPreset( Preset existingPreset )
        {
            OnCantCreateNewPreset( existingPreset.IsModifiedPreset() 
                                       ? CantCreateNewPresetReasons.NewPresetModified 
                                       : CantCreateNewPresetReasons.NewPresetNotModified, 
                                   existingPreset );
        }

        private void OnCantCreateNewPreset(CantCreateNewPresetReasons reason, Preset preset)
        {
            CantCreateNewPresetEvent?.Invoke(this, new CantCreateNewPreetEventArgs(reason, preset));
        }

        private void RemoveDefaultPresets()
        {
            var defaultNamedPresets = GetDefaultNamedPresets();

            if (defaultNamedPresets.Length > 0) {
                foreach (var preset in defaultNamedPresets) {
                    PresetCollection.Remove(preset);
                }
            }
        }

        #endregion
    }
}
