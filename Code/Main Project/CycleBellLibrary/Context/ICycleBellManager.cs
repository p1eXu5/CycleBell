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

        /// <summary>
        /// Creates a new preset
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        bool CreateNewPreset();

        void CheckCreateNewPreset(Preset existEmptyPreset);

        void OpenPresets (string fileName);

        void DeletePreset (Preset preset);

        void ClearPresets();

        void RenamePreset (Preset preset, string newName);

        void SavePresets (string fileName);

        event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;

        bool IsNewPreset(Preset preset);

        void RemovePresets(Preset[] presets);
        void RemoveNewPresets();
    }
}
