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
using System.Windows.Input;
using CycleBell.Annotations;
using CycleBell.Base;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public abstract class TimePointViewModelBase : ViewModel, IEquatable<TimePointViewModelBase>
    {
        #region Fields

        private int _id;
        private byte _loopNumber;

        protected IPresetViewModel _PresetViewModel;

        private bool _isEnabled = true;
        private bool _isActive = false;
        private bool _moveFocusNext;

        #endregion

        #region Constructors

        protected TimePointViewModelBase (int id, byte loopNumber, IPresetViewModel presetViewModel)
        {
            _PresetViewModel = presetViewModel;

            _id = id;
            _loopNumber = loopNumber;

            TimePointNameInTemplateReturnCommand = new ActionCommand(TimePointNameReturn);
        }

        #endregion

        public ICommand TimePointNameInTemplateReturnCommand { get; }

        #region Properties

        public abstract TimePoint TimePoint { get; }

        public virtual int Id
        {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged();
            }
        }
        public virtual byte LoopNumber
        {
            get => _loopNumber;
            set {
                _loopNumber = value;
                OnPropertyChanged();
            }
        }

        public virtual bool IsEnabled
        {
            get => _isEnabled;
            set {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool MoveFocusNext
        {
            get => _moveFocusNext;
            set {
                _moveFocusNext = value;
                OnPropertyChanged();
            }
        }

        #endregion

        private void TimePointNameReturn(object o)
        {
            MoveFocusNext ^= true;
        }

        #region Operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals (null, obj)) return false;
            if (ReferenceEquals (this, obj)) return true;
            if (this.GetType() != obj.GetType()) return false;

            return Equals ((TimePointViewModelBase) obj);
        }

        public bool Equals (TimePointViewModelBase other)
        {
            if (ReferenceEquals (null, other)) return false;
            if (ReferenceEquals (this, other)) return true;

            return Id == other.Id && LoopNumber == other.LoopNumber;
        }

        public bool Equals (TimePoint timePoint)
        {
            if (TimePoint == null) return timePoint == null;
            if (timePoint == null) return TimePoint == null;
            if (ReferenceEquals (this.TimePoint, timePoint)) return true;

            return TimePoint.Equals(timePoint);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        #endregion
    }
}