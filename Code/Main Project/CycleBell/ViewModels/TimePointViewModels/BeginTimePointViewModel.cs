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
using System.ComponentModel.DataAnnotations;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class BeginTimePointViewModel : TimePointViewModelBase
    {
        public BeginTimePointViewModel(byte loopNumber, IPresetViewModel presetViewModel) : base((int)TimePoint.MinId, loopNumber, presetViewModel)
        { }

        public override TimePoint TimePoint => null;

        public string CycleName => $"loop {LoopNumber}";

        [Range(1, Int32.MaxValue, ErrorMessage = "Loop count must be greater then 0")]
        public int LoopCount
        {
            get => _PresetViewModel.Preset.TimerLoops[LoopNumber];
            set {
                _PresetViewModel.Preset.TimerLoops[LoopNumber] = value;
                OnPropertyChanged();
            }
        }
    }
}
