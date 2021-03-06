﻿/*
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
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CycleBell.Base;
using CycleBell.Engine.Models;
using CycleBell.Engine.Models.Extensions;
using CycleBell.Engine.Timer;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBell.Views;

[assembly: InternalsVisibleTo("CycleBell.Tests")]
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
        #region fields

        private readonly IMainViewModel _mainViewModel;

        private readonly ObservableCollection<TimePointViewModelBase> _timePointVmCollection;
        private AddingTimePointViewModel _addingTimePoint;

        private bool _canBellOnStartTime;

        private readonly HashSet<int> _settedLoopNumbers;

        private string _nextTimePointName;
        private TimeSpanDigits _timeLeftTo;

        private TimePointViewModelBase _activeTimePointVm;

        private bool _focusStartTime;

        private readonly Dispatcher _dispatcher;

        #endregion


        #region ctor

        public PresetViewModel(Preset preset, IMainViewModel mainViewModel)
        {
            #region local functions

            void LoadTimePointViewModelCollection()
            {
                if (Preset.TimePointCollection.Count > 0) 
                {
                    foreach (var point in Preset.TimePointCollection) 
                    {
                        _timePointVmCollection.Add( GetTimePointViewModel( point ) );

                        if ( !_settedLoopNumbers.Contains( point.LoopNumber ) ) {
                            AddVisualBounds (point.LoopNumber);
                        }
                    }
                }
            }

            void SetupTimePointVmCollectionView()
            {
                ICollectionView view = CollectionViewSource.GetDefaultView( TimePointVmCollection );

                view.SortDescriptions.Clear();
                view.SortDescriptions.Add( new SortDescription( "LoopNumber", ListSortDirection.Ascending ) );
                view.SortDescriptions.Add( new SortDescription( "Id", ListSortDirection.Ascending ) );
            }

            void SetupCanBellOnStartTime()
            {
                if ( Preset.Tag is string str ) {
                    if ( str == "true" )
                        CanBellOnStartTime = true;
                    else if ( str == "false" )
                        CanBellOnStartTime = false;
                }
            }

            AddingTimePointViewModel GetAddingTimePointViewModel ()
            {
                var addingTimePoint = new AddingTimePointViewModel (this);
                addingTimePoint.Reset();

                ((INotifyPropertyChanged) addingTimePoint).PropertyChanged += OnTimePropertyChanged;

                return addingTimePoint;
            }

            #endregion


            Preset = preset ?? throw new ArgumentNullException(nameof(preset));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            _settedLoopNumbers = new HashSet<int>();

            _timePointVmCollection = new ObservableCollection<TimePointViewModelBase>();
            LoadTimePointViewModelCollection();
            
            TimePointVmCollection = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePointVmCollection);
            
            SetupTimePointVmCollectionView();
            SetupCanBellOnStartTime();

            AddingTimePoint = GetAddingTimePointViewModel();

            ((INotifyCollectionChanged) Preset.TimePointCollection).CollectionChanged += OnTimePointCollectionChanged;

            _dispatcher = Application.Current?.Dispatcher;
        }

        #endregion


        #region properties

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

        public bool IsNew => _mainViewModel.IsNewPreset( Preset );
        public bool IsModified => Preset.IsModifiedPreset();

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
        public bool HasTimePoints => TimePointVmCollection.Count > 0;

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

        public ICommand SetStartTimeCommand => new ActionCommand( SetStartTime );

        #endregion


        #region Methods

        private void SetStartTime( object offset )
        {
            StartTime = (DateTime.Now + TimeSpan.FromMinutes( Int32.Parse( offset.ToString() ) )).TimeOfDay;
        }

        private void OnTimePointCollectionChanged (Object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add
            if (e.NewItems?[0] is TimePoint newTimePoint) {

                _timePointVmCollection.Add (GetTimePointViewModel(newTimePoint));
                if ( !_settedLoopNumbers.Contains( newTimePoint.LoopNumber ) ) {
                    AddVisualBounds (newTimePoint.LoopNumber);
                }
            }
            // Remove
            else if (e.OldItems?[0] is TimePoint oldTimePoint) {

                var loopNumber = oldTimePoint.LoopNumber;
                var timePoints = _timePointVmCollection.Where (tpvm => tpvm.LoopNumber == loopNumber).ToArray();

                if (timePoints.Length > 3) {

                    var removingTimePointVm = timePoints.First (tpvm => tpvm.Id == oldTimePoint.Id);
                    _timePointVmCollection.Remove( removingTimePointVm );
                    _mainViewModel.Alarm.RemoveSound( (TimePointViewModel)removingTimePointVm );
                }
                else if (timePoints.Length <= 3) {

                    foreach (var timePointViewModel in timePoints) {
                        _timePointVmCollection.Remove( timePointViewModel );
                        if ( timePointViewModel is TimePointViewModel tpvm ) {
                            _mainViewModel.Alarm.RemoveSound( tpvm );
                        }
                    }

                    _settedLoopNumbers.Remove(loopNumber);
                }
            }

            OnPropertyChanged (nameof(HasTimePoints));
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
                timePoint.Name = TimePoint.GetDefaultName(timePoint);

            Preset.AddTimePoint(timePoint);

            ResetAddingTimePoint();
        }
        private bool CanAddTimePoint (object o)
        {
            var res = _addingTimePoint.Time < TimeSpan.FromDays(1) 
                      && ((_addingTimePoint.TimePointKinds == TimePointKinds.Relative && _addingTimePoint.Time > TimeSpan.Zero) 
                      || (_addingTimePoint.TimePointKinds == TimePointKinds.Absolute && _addingTimePoint.Time >= TimeSpan.Zero));

            return res;
        }
        private bool CanAddTimePoint (TimePoint timePoint)
        {
            var res = timePoint.Time == TimeSpan.Zero && timePoint.Kind == TimePointKinds.Absolute 
                      || timePoint.Time > TimeSpan.Zero;

            return res;
        }

        // SoundBank
        public void UpdateSoundBank (TimePoint timePoint)
        {
            _mainViewModel.Alarm.AddSound( timePoint );
        }

        // TimerManager handlers:
        internal void OnTimePointChanged(object s, TimerEventArgs e)
        {
            if (e == null) return;

            if ( _dispatcher != null ) {
                _dispatcher.BeginInvoke( DispatcherPriority.Normal, (Action< TimerEventArgs >)Ring, e );
            }
            else {
                Ring( e );
            }

            if (e.NextTimePoint == null) return;
            UpdateActiveTimePointViewModel (e.NextTimePoint, e.PrevTimePointNextBaseTime);
            UpdateTimerCounter (e);
        }

        private void Ring( TimerEventArgs e )
        {
            //Trace.WriteLine( $"prev: {e.PrevTimePoint}; next: {e.NextTimePoint}" );

            if ( TimerManager.IsInitialTimePoint( e.PrevTimePoint )) {
            }
            else if ( TimerManager.IsStartTimePoint( e.PrevTimePoint, Preset )) {
                if ( _mainViewModel.IsRingOnStartTime ) {
                    _mainViewModel.Alarm.Play();
                }
            }
            else  {
                _mainViewModel.Alarm.Play();
            }

            _mainViewModel.Alarm.LoadNextSound( e.NextTimePoint );
        }

        private void UpdateTimerCounter(TimerEventArgs e)
        {
            NextTimePointName = e.NextTimePoint.Name;
            TimeLeftTo = TimeSpanDigits.Parse (-e.LastTimeToNextChange);
        }

        private void UpdateActiveTimePointViewModel(TimePoint nextTimePoint, TimeSpan? prevTimePointNextBaseTime)
        {
            // deactivate:
            if (_activeTimePointVm != null) {

                _activeTimePointVm.IsActive = false;

                if (prevTimePointNextBaseTime != null) {

                    _activeTimePointVm.TimePoint.BaseTime = prevTimePointNextBaseTime;
                    ((TimePointViewModel)_activeTimePointVm).UpdateTime();
                }
            }

            // activate:
            if (nextTimePoint.Name == _mainViewModel.StartTimeName) {

                if (TimePointVmCollection.Count > 0) {

                    TimePointVmCollection.DisableAll();
                    ResetBaseTimes();
                }
            }
            else {
                _activeTimePointVm = TimePointVmCollection.Activate(tpvmb => tpvmb.Equals(nextTimePoint));
            }

            //(( DispatcherObject )_mainViewModel.Alarm.Player).Dispatcher.BeginInvoke( DispatcherPriority.Normal, (ThreadStart)delegate() { _mainViewModel.Alarm.LoadNextSound( nextTimePoint ); } );
            
        }

        internal void OnSecondPassed(object s, TimerEventArgs e)
        {
            TimeLeftTo = TimeSpanDigits.Parse(-e.LastTimeToNextChange);
        }

        internal void OnTimerPaused(object sender, EventArgs args)
        {
            if ( _dispatcher != null ) {
                _dispatcher.BeginInvoke( DispatcherPriority.Normal, (Action)_mainViewModel.Alarm.Stop );
            }
            else {
                _mainViewModel.Alarm.Stop();
            }
        }

        internal void OnTimerStopped(object sender, EventArgs args)
        {
            if (_activeTimePointVm != null)
                _activeTimePointVm.IsActive = false;

            TimePointVmCollection.EnableAll();

            ResetBaseTimes();
        }


        private void ResetBaseTimes()
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
        /// <param name="loopNumber"></param>
        private void AddVisualBounds( int loopNumber )
        {
            if (_settedLoopNumbers.Contains( loopNumber ) )
                return;

            _timePointVmCollection.Add (new BeginTimePointViewModel (loopNumber, this));
            _timePointVmCollection.Add (new EndTimePointViewModel (loopNumber));

            _settedLoopNumbers.Add(loopNumber);
        }

        /// <summary>
        /// Checks TimePoint conditions
        /// </summary>
        /// <param name="point"></param>
        private TimePointViewModel GetTimePointViewModel( TimePoint point )
        {
            if (point.Time < TimeSpan.Zero) {
                point.Time = point.Time.Negate();
            }

            var tp = new TimePointViewModel (point, this);

            if (point.Kind == TimePointKinds.Absolute) {

                point.ChangeTimePointType( TimePointKinds.Relative );
                tp.IsAbsolute = true;
            }

            UpdateSoundBank( tp.TimePoint );

            return tp;
        }

        internal void RaiseOnIsNoTimePointChanged() => OnPropertyChanged(nameof(IsNoTimePoints));

        #endregion

    }
}
