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
using CycleBell.Engine.Models;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;

namespace CycleBell.Engine
{
    public interface ICycleBellManager
    {
        IPresetCollection PresetCollection { get; }
        ITimerManager TimerManager { get; }

        event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;

        string FileName { get; }

        /// <summary>
        /// Creates and adds a new existingPreset when equaled a new existingPreset doesn't
        /// exiPresetCollection cref="PresetCollection.Presets"/> presetCollection.
        /// </summary>
        /// <returns><see cref="Boolean"/> if existingPreset added</returns>
        bool CreateNewPreset();
        bool AddPreset( Preset preset );
        void RenamePreset (Preset preset, string newName);
        void ClearPresets();
        void RemovePreset(Preset preset);
        bool IsNewPreset(Preset preset);
        void OpenPresets (string fileName);
        void OpenPresets();
        void SavePresets (string fileName);
    }
}
