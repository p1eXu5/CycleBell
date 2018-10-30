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

        void CheckCreateNewPreset(Preset existEmptyPreset);
        void ClearPresets();
        bool CreateNewPreset();
        void DeletePreset (Preset preset);
        bool IsNewPreset(Preset preset);
        void OpenPresets (string fileName);
        void RemoveNewPresets();
        void RemovePresets(Preset[] presets);
        void RenamePreset (Preset preset, string newName);
        void SavePresets (string fileName);

        event EventHandler<CantCreateNewPreetEventArgs> CantCreateNewPresetEvent;



    }
}
