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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using CycleBell.Base;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBell.Views;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Timer;

namespace CycleBell.ViewModels
{
    [Flags]
    public enum CycleBellStateFlags : byte
    {
        FirstCall = 0x01,
        InfiniteLoop = 0x02
    }

    /// <summary>
    /// Timer preset
    /// </summary>
    public class PresetViewModel : ObservableObject, IPresetViewModel
    {
        #region Fields
        private readonly IMainViewModel _mainViewModel;

        private readonly ObservableCollection<TimePointViewModelBase> _timePointVmCollection;
        //private TimePointViewModelBase _selectedTimePoint;
        private AddingTimePointViewModel _addingTimePoint;

        private bool _canBellOnStartTime;

        private readonly HashSet<int> _settedLoopNumbers;

        private string _nextTimePointName;
        private TimeSpanDigits _timeLeftTo;

        public IDictionary<int, SoundPlayer> SoundMap { get; } = new Dictionary<int, SoundPlayer>();
        private SoundPlayer _lastSoundPlayer;

        private TimePointViewModelBase _activeTimePointViewModelBase;

        private bool _focusStartTime;

        #endregion

        #region Constructor

        public PresetViewModel(Preset preset, IMainViewModel mainViewModel)
        {
            // _PresetViewModel
            Preset = preset ?? throw new ArgumentNullException(nameof(preset));

            // _presetCollectionManager
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));


            // _settedLoopNumbers
            _settedLoopNumbers = new HashSet<int>();

            // _timePointVmCollection
            _timePointVmCollection = new ObservableCollection<TimePointViewModelBase>();
            LoadTimePointViewModelCollection(Preset);
            
            // TimePointVmCollection
            TimePointVmCollection = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePointVmCollection);

            // CollectionView:
            ICollectionView view = CollectionViewSource.GetDefaultView (TimePointVmCollection);

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add (new SortDescription("LoopNumber", ListSortDirection.Ascending));
            view.SortDescriptions.Add (new SortDescription ("Id", ListSortDirection.Ascending));

            // TimePointCollection INotifyCollectionChanged
            ((INotifyCollectionChanged) Preset.TimePointCollection).CollectionChanged += OnTimePointCollectionChanged;

            // CanBellOnStartTime
            if (Preset.Tag is string str) {

                if (str == "true")
                    CanBellOnStartTime = true;
                else if (str == "false")
                    CanBellOnStartTime = false;
            }


            // AddingTimePoint
            AddingTimePoint = GetAddingTimePointViewModel();
        }

        #endregion

        #region Properties

        // Preset
        public Preset Preset { get; }
        public string Name
        {
            get => Preset?.PresetName;
            set {
                Preset.PresetName = value;
                OnPropertyChanged ();
            }
        }

        public TimeSpan StartTime
        {
            get => Preset.StartTime;
            set {
                Preset.StartTime = value;
                OnPropertyChanged();
                //OnPropertyChanged(nameof(TimePointVmCollection));
                foreach (var timePointViewModel in TimePointVmCollection.Where(t => t is TimePointViewModel).ToArray()) {
                    ((TimePointViewModel)timePointViewModel).UpdateTime();
                }
            }
        }

        public ReadOnlyObservableCollection<TimePointViewModelBase> TimePointVmCollection { get; }
        //public TimePointViewModelBase SelectedTimePoint
        //{
        //    get => _selectedTimePoint;
        //    set {
        //        if (value is TimePointViewModel newValue) {
        //            _selectedTimePoint = newValue;
        //        }
        //        OnPropertyChanged ();
        //    }
        //}
        public AddingTimePointViewModel AddingTimePoint
        {
            get => _addingTimePoint;
            set {
                _addingTimePoint = value;
                OnPropertyChanged ();
            }
        }

        public bool IsNew => PresetChecker.IsNewPreset (Preset);
        public bool IsModified => PresetChecker.IsModifiedPreset (Preset);

        public bool IsInfiniteLoop
        {
            get => Preset.IsInfiniteLoop;
            set {
                if (value) {
                    Preset.SetInfiniteLoop();
                }
                else {
                    Preset.ResetInfiniteLoop();
                }
                OnPropertyChanged ();
            }
        }
        public bool CanBellOnStartTime
        {
            get => _canBellOnStartTime;
            set {
                _canBellOnStartTime = value;
                OnPropertyChanged ();
            }
        }


        public bool IsNoTimePoints => TimePointVmCollection.Count < 1;
        public bool IsTimePoints => TimePointVmCollection.Count > 0;

        public string NextTimePointName
        {
            get => _nextTimePointName;
            set {
                _nextTimePointName = value;
                OnPropertyChanged();
            }
        }

        public TimeSpanDigits TimeLeftTo
        {
            get => _timeLeftTo;
            set {
                _timeLeftTo = value;
                OnPropertyChanged();
            }
        }

        public bool FocusStartTime
        {
            get => _focusStartTime;
            set {
                _focusStartTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand AddTimePointCommand => new ActionCommand (AddTimePoint, CanAddTimePoint);

        #endregion

        #region Methods

        private void OnTimePointCollectionChanged (Object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add
            if (e.NewItems?[0] is TimePoint newTimePoint) {

                //var newTimePoint = (TimePoint) e.NewItems[0];

                PrepareTimePoint(newTimePoint);
                _timePointVmCollection.Add (new TimePointViewModel (newTimePoint, this));

                CheckBounds (newTimePoint);

                OnPropertyChanged(nameof(IsTimePoints));
                return;
            }

            // Remove
            if (e.OldItems?[0] is TimePoint oldTimePoint) {

                var loopNumber = oldTimePoint.LoopNumber;
                var timePoints = _timePointVmCollection.Where (tpvm => tpvm.LoopNumber == loopNumber).ToArray();

                if (timePoints.Length == 3) {

                    _timePointVmCollection.Remove(timePoints[0]);
                    _timePointVmCollection.Remove(timePoints[1]);
                    _timePointVmCollection.Remove(timePoints[2]);

                    _settedLoopNumbers.Remove(loopNumber);

                    OnPropertyChanged(nameof(IsTimePoints));
                    return;
                }

                var removingTimePointVm = timePoints.First (tpvm => tpvm.Id == oldTimePoint.Id);
                _timePointVmCollection.Remove (removingTimePointVm);
            }

        }

        // Service:
        private void LoadTimePointViewModelCollection (Preset preset)
        {
            if (preset.TimePointCollection.Count > 0) {

                foreach (var point in preset.TimePointCollection) {

                    PrepareTimePoint (point);
                    _timePointVmCollection.Add (new TimePointViewModel (point, this));

                    CheckBounds (point);
                }
            }
        }

        private AddingTimePointViewModel GetAddingTimePointViewModel ()
        {
            var addingTimePoint = new AddingTimePointViewModel (this);
            addingTimePoint.Reset();

            ((INotifyPropertyChanged) addingTimePoint).PropertyChanged += OnTimePropertyChanged;

            return addingTimePoint;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void ResetAddingTimePoint()
        {
            AddingTimePoint.Reset();

            // Update TimePoint list trigger
            OnPropertyChanged (nameof(IsNoTimePoints));
        }
        

        private void OnTimePropertyChanged (object s, PropertyChangedEventArgs e)
        {
            OnPropertyChanged (nameof(CanAddTimePoint));
        }

        // RemoveTimePoint
        public void RemoveTimePoint (TimePoint timePoint)
        {
            Preset.RemoveTimePoint (timePoint);
        }

        // AddTimePointCommand:
        private void AddTimePoint(object o)
        {
            var timePoint = _addingTimePoint.TimePoint.Clone();

            if (String.IsNullOrWhiteSpace (timePoint.Name))
                timePoint.Name = TimePoint.GetDefaultTimePointName(timePoint);

            Preset.AddTimePoint(timePoint);
            timePoint.ChangeTimePointType(TimePointType.Relative);

            ResetAddingTimePoint();
        }
        private bool CanAddTimePoint (object o)
        {
            var res = _addingTimePoint.Time < TimeSpan.FromDays(1) 
                      && ((_addingTimePoint.TimePointType == TimePointType.Relative && _addingTimePoint.Time > TimeSpan.Zero) 
                      || (_addingTimePoint.TimePointType == TimePointType.Absolute && _addingTimePoint.Time >= TimeSpan.Zero));

            return res;
        }
        private bool CanAddTimePoint (TimePoint timePoint)
        {
            var res = timePoint.Time == TimeSpan.Zero && timePoint.TimePointType == TimePointType.Absolute 
                      || timePoint.Time > TimeSpan.Zero;

            return res;
        }

        // SoundBank
        public void UpdateSoundBank (TimePoint timePoint)
        {
            SoundMap[timePoint.Id] = new SoundPlayer((string)timePoint.Tag);
        }

        private void Ring(TimePoint prevTimePoint)
        {
            if (prevTimePoint != null && prevTimePoint.Time >= TimeSpan.Zero) {
                if (prevTimePoint.Name == _mainViewModel.StartTimeName && _mainViewModel.IsRingOnStartTime) {
                    _mainViewModel.Ring();
                }
                else {

                    // TODO (and mute)
                    if (_activeTimePointViewModelBase != null) {

                        if (!((TimePointViewModel) _activeTimePointViewModelBase).MuteFlag) {

                            if (SoundMap.ContainsKey(_activeTimePointViewModelBase.Id)) {
                                SoundMap[prevTimePoint.Id].Play();
                                _lastSoundPlayer = SoundMap[prevTimePoint.Id];
                            }
                            else {
                                _mainViewModel.Ring();
                            }
                        }
                    }
                }
            }
        }

        // TimerManager handlers:
        internal void OnTimePointChangedEventHandler(object s, TimerEventArgs e)
        {
            if (e == null) return;
            Ring(e.PrevTimePoint);

            if (e.NextTimePoint == null) return;
            UpdateActiveTimePointViewModel (e.NextTimePoint, e.PrevTimePointNextBaseTime);
            UpdateUiTimerProperties (e);
        }

        private void UpdateUiTimerProperties(TimerEventArgs e)
        {
            NextTimePointName = e.NextTimePoint.Name;
            TimeLeftTo = TimeSpanDigits.Parse (-e.LastTimeToNextChange);
        }

        private void UpdateActiveTimePointViewModel(TimePoint nextTimePoint, TimeSpan? prevTimePointNextBaseTime)
        {
            // deactivate:
            if (_activeTimePointViewModelBase != null) {

                _activeTimePointViewModelBase.IsActive = false;

                if (prevTimePointNextBaseTime != null) {

                    _activeTimePointViewModelBase.TimePoint.BaseTime = prevTimePointNextBaseTime;

                    ((TimePointViewModel)_activeTimePointViewModelBase).UpdateTime();
                }
            }

            // activate:
            if (nextTimePoint.Name == _mainViewModel.StartTimeName) {
                if (TimePointVmCollection.Count > 0) {
                    TimePointVmCollection.DisableAll();
                }
            }
            else {
                _activeTimePointViewModelBase = TimePointVmCollection.Activate(tpvmb => tpvmb.Equals(nextTimePoint));
            }
        }

        internal void OnSecondPassedEventHandler(object s, TimerEventArgs e)
        {
            TimeLeftTo = TimeSpanDigits.Parse(-e.LastTimeToNextChange);
        }

        internal void OnTimerPauseEventHandler(object sender, EventArgs args)
        {
            _mainViewModel.Ring(true);
            _lastSoundPlayer?.Stop();
        }

        internal void OnTimerStopEventHandler(object sender, EventArgs args)
        {
            if (_activeTimePointViewModelBase != null)
                _activeTimePointViewModelBase.IsActive = false;

            TimePointVmCollection.EnableAll();

            UpdateBaseTimes();
        }

        private void UpdateBaseTimes()
        {
            Preset.UpdateTimePointBaseTimes();
            foreach (var viewModel in TimePointVmCollection.Where(tp => tp is TimePointViewModel).ToArray()) {

                ((TimePointViewModel)viewModel).UpdateTime();
            }
        }

        // Checkers:
        /// <summary>
        /// Used in OnTimePointCollectionChanged
        /// </summary>
        /// <param name="timePoint"></param>
        private void CheckBounds(TimePoint timePoint)
        {
            if (_settedLoopNumbers.Contains (timePoint.LoopNumber))
                return;

            _timePointVmCollection.Add (new BeginTimePointViewModel (timePoint.LoopNumber, this));
            _timePointVmCollection.Add (new EndTimePointViewModel (timePoint.LoopNumber));

            _settedLoopNumbers.Add (timePoint.LoopNumber);
        }

        /// <summary>
        /// Checks TimePoint conditions
        /// </summary>
        /// <param name="point"></param>
        private static void PrepareTimePoint (TimePoint point)
        {
            if (point.Time < TimeSpan.Zero)
                point.Time = point.Time.Negate();

            if (point.TimePointType == TimePointType.Absolute)
                point.ChangeTimePointType(TimePointType.Relative);
        }

        internal void RaiseOnIsNoTimePointChanged() => OnPropertyChanged(nameof(IsNoTimePoints));

        #endregion

    }
}
