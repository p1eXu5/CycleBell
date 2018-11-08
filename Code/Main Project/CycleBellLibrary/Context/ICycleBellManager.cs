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
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary.Context
{
    public interface ICycleBellManager
    {
        IPresetCollectionManager PresetCollectionManager { get; }
        ITimerManager TimerManager { get; }

        Preset GetNewPreset();

        void RiseCantCreateNewPreset(Preset existNewPreset);
        void ClearPresets();

        /// <summary>
        /// Creates and adds a new preset when equaled a new preset doesn't
        /// exist in <see cref="Repository.PresetCollectionManager.Presets"/> collection.
        /// </summary>
        /// <returns><see cref="Boolean"/> if preset added</returns>
        bool CreateNewPreset();

        bool IsNewPreset(Preset preset);
        void OpenPresets (string fileName);
        void RemoveNewPresets();
        void RemovePreset(Preset preset);
        void RemovePresets(Preset[] presets);
        void RenamePreset (Preset preset, string newName);
        void SavePresets (string fileName);

        event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;



    }
}
