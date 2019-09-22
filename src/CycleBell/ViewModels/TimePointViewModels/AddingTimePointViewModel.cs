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
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Windows.Input;
using CycleBell.Base;
using CycleBell.Engine.Models;
using Microsoft.Win32;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class AddingTimePointViewModel : TimePointViewModel
    {
        private const int _LOOP_NUMBER_LIMIT = 10;
        private bool _focusTime;
        private bool _focusName;
        private bool _isAbsolute;

        public AddingTimePointViewModel (IPresetViewModel presetViewModel) : base (new TimePoint { Name = "" }, presetViewModel)
        {
            AddTimePointCommand = new ActionCommand (AddTimePoint, _PresetViewModel.AddTimePointCommand.CanExecute);

            NumberCollection = _PresetViewModel.Preset != null 
                               && _PresetViewModel.Preset.TimerLoopDictionary.Values.Any() 
                               && _PresetViewModel.Preset.TimerLoopDictionary.Values.Max() > _LOOP_NUMBER_LIMIT 
                                    ? Enumerable.Range (0, _PresetViewModel.Preset.TimerLoopDictionary.Values.Max() + 1).Select (n => (int) n).ToArray() 
                                    : Enumerable.Range(0, _LOOP_NUMBER_LIMIT).Select(n => (int)n).ToArray();
        }

        #region properties

        public override TimeSpan Time
        {
            get => TimePoint.Time;
            set {
                TimePoint.Time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoSetTime));
                ((ActionCommand)AddTimePointCommand).RaiseCanExecuteChanged();
            }
        }

        public override string Name
        {
            get => base.Name;
            set {
                base.Name = value;
                OnPropertyChanged(nameof(HasNoName));
            }
        }

        public int[] NumberCollection { get; private set; }

        public override bool IsAbsolute
        {
            get => _isAbsolute;
            set {
                _isAbsolute = value;

                if (_isAbsolute) {
                    TimePoint.ChangeTimePointType(TimePointKinds.Absolute);
                }
                else {
                    TimePoint.ChangeTimePointType(TimePointKinds.Relative);
                }

                FocusTime ^= true;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(NoSetTime));
            }
        }

        public bool HasNoName => String.IsNullOrWhiteSpace(Name);

        public bool NoSetTime => Time == TimeSpan.Zero && TimePointKinds == TimePointKinds.Relative;

        public bool FocusTime
        {
            get => _focusTime;
            set {
                _focusTime = value;
                OnPropertyChanged();
            }
        }

        public bool FocusName
        {
            get => _focusName;
            set {
                _focusName = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region Commands

        public ICommand AddTimePointCommand { get; }
        public ICommand TimePointNameReturnCommand => new ActionCommand(TimePointNameReturn);

        #endregion

        private void TimePointNameReturn(object o)
        {
            FocusTime ^= true;
        }

        private void AddTimePoint(object o)
        {
            _PresetViewModel.AddTimePointCommand.Execute(null);
            IsAbsolute = false; 
            FocusName ^= true;
        }

        public void Reset()
        {
            Name = "";
            Time = TimeSpan.Zero;

            TimePoint.ChangeTimePointType(TimePointKinds.Relative);
            OnPropertyChanged(nameof(TimePointKinds));

            LoopNumber = 0;
        }

        public void CopyFrom(TimePoint timePoint)
        {
            TimePoint.Name = timePoint.Name;
            TimePoint.Time = timePoint.Time;
            TimePoint.ChangeTimePointType(timePoint.Kind);
            TimePoint.LoopNumber = timePoint.LoopNumber;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(TimePointKinds));
            OnPropertyChanged(nameof(LoopNumber));
        }

        protected override void OpenWavFile()
        {
             var ofd = new OpenFileDialog {
                Filter = "mp3, wav|*.mp3;*.wav|all files|*.*",
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyMusic ),
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog() == true) {

                TimePoint.Tag = ofd.FileName;
                OnPropertyChanged(nameof(SoundLocation));
            }
        }

    }
}
