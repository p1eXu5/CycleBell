using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using CycleBell.Base;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBellLibrary;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
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
    public class PresetViewModel : ObservableObject
    {
        #region Consts

        private const string StarString = "*";
        private const string NewString = "new";

        #endregion

        #region Fields

        private readonly Preset _preset;
        private readonly ITimerManager _timerManager;
        private readonly IPresetsManager _presetsManager;

        private readonly ObservableCollection<TimePointViewModelBase> _timePointVmCollection;
        private TimePointViewModelBase _selectedTimePoint;
        private AddingTimePointViewModel _addingTimePoint;

        private bool _isModified;
        private bool _canBellOnStartTime;

        private readonly HashSet<byte> _settedLoopNumbers;

        private StringBuilder _name;
        #endregion

        #region Constructor

        public PresetViewModel(Preset preset, ICycleBellManager manager)
        {
            // _preset
            _preset = preset ?? throw new ArgumentNullException(nameof(preset));

            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            // _presetsManager
            _presetsManager = manager.PresetsManager ?? throw new ArgumentNullException(nameof(PresetsManager));

            // _timerManager and handlers
            _timerManager = GetTimerManager(manager);

            // _settedLoopNumbers
            _settedLoopNumbers = new HashSet<byte>();

            // _timePointVmCollection
            _timePointVmCollection = GetTimePointViewModelCollection(_preset);
            
            // TimePointVmCollection
            TimePointVmCollection = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePointVmCollection);

            // CollectionView:
            ICollectionView view = CollectionViewSource.GetDefaultView (TimePointVmCollection);

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add (new SortDescription ("Id", ListSortDirection.Ascending));
            view.SortDescriptions.Add (new SortDescription("LoopNumber", ListSortDirection.Ascending));

            // TimePoints INotifyCollectionChanged
            ((INotifyCollectionChanged) _preset.TimePoints).CollectionChanged += UpdateTimePointVmCollection;

            // CanBellOnStartTime
            if (_preset.Tag is string str) {

                if (str == "true")
                    CanBellOnStartTime = true;
                else if (str == "false")
                    CanBellOnStartTime = false;
            }

            // _name
            _name = new StringBuilder();
            _name.Append(GetPresetName(_preset.PresetName));

            // AddingTimePoint
            AddingTimePoint = new AddingTimePointViewModel (TimePoint.DefaultTimePoint, _preset, AddTimePointCommand);
            ((INotifyPropertyChanged) AddingTimePoint).PropertyChanged += OnTimePropertyChanged;
        }

        #endregion

        #region Properties

        // Preset
        public Preset Preset => _preset;
        public string Name => _name.ToString();

        public TimeSpan StartTime
        {
            get => _preset.StartTime;
            set {
                _preset.StartTime = value;
                OnPropertyChanged();
            }
        }

        public ReadOnlyObservableCollection<TimePointViewModelBase> TimePointVmCollection { get; }
        public TimePointViewModelBase SelectedTimePoint
        {
            get => _selectedTimePoint;
            set {
                if (value is TimePointViewModel newValue) {
                    _selectedTimePoint = newValue;
                }
                OnPropertyChanged ();
            }
        }
        public AddingTimePointViewModel AddingTimePoint
        {
            get => _addingTimePoint;
            set {
                _addingTimePoint = value;
                OnPropertyChanged ();
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set {
                if (value == true)
                    AddStarToName();
            }
        }

        public bool IsInfiniteLoop
        {
            get => _preset.IsInfiniteLoop;
            set {
                if (value) {
                    _preset.SetInfiniteLoop();
                }
                else {
                    _preset.ResetInfiniteLoop();
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

        public bool IsNewPreset => _preset.PresetName == Preset.PresetName;
        public bool IsNoTimePoints => TimePointVmCollection.Count < 1;

        // ITimerManager
        public bool IsRunning => _timerManager.IsRunning;

        #endregion

        #region Commands

        public ICommand AddTimePointCommand => new ActionCommand (AddTimePoint, CanAddTimePoint);
        public ICommand PlayCommand => new ActionCommand (Play, CanPlay);
        public ICommand PouseCommand => new ActionCommand(Pouse, CanPouse);
        public ICommand ResumeCommand => new ActionCommand (Resume, CanResume);
        public ICommand StopCommand => new ActionCommand(Stop, CanStop);
        public ICommand InfiniteToggleCommand => new ActionCommand (ToggleInfinite, CanToggleInfinite);
        public ICommand BellOnStartTimeToggleCommand => new ActionCommand (BellOnStartTime);
        public ICommand RingCommand => new ActionCommand(Ring);

        #endregion

        #region Methods

        // Service:
        private ITimerManager GetTimerManager (ICycleBellManager manager)
        {
            var timerManager = manager.TimerManager ?? throw new ArgumentNullException(nameof(TimerManager));
            timerManager.TimerSecondPassedEvent += OnSecondPassed;
            timerManager.ChangeTimePointEvent += OnTimePointChanged;
            timerManager.TimerStopEvent += OnStop;

            return timerManager;
        }

        private ObservableCollection<TimePointViewModelBase> GetTimePointViewModelCollection (Preset preset)
        {
            var timePointVmCollection = new ObservableCollection<TimePointViewModelBase>();

            if (preset.TimePoints.Count > 0) {

                foreach (var point in preset.TimePoints) {

                    PrepareTimePoint (point);
                    timePointVmCollection.Add (new TimePointViewModel (point, preset));

                    CheckBounds (point);
                }
            }

            return timePointVmCollection;
        }

        private void AddStarToName()
        {
            _name.Clear();

            if (IsNewPreset) {
                _name.Append(NewString);
            }

            _name.Append(StarString);

            OnPropertyChanged(nameof(Name));
        }

        private string GetPresetName(string presetName)
        {
            if (_preset.PresetName == Preset.DefaultName) {
                return NewString;
            }
            else
                return presetName;
        }

        private void OnTimePropertyChanged (object s, PropertyChangedEventArgs e)
        {
            OnPropertyChanged (nameof(CanAddTimePoint));
        }

        public PresetViewModel GetDeepCopy()
        {
            var presetVm = (PresetViewModel) this.MemberwiseClone();
            //presetVm._preset = _preset.GetDeepCopy();

            return null;
        }

        protected internal void Save()
        {
            throw new NotImplementedException();
        }

        protected internal bool CanSave()
        {
            return IsModified;
        }

        private static void PrepareTimePoint (TimePoint point)
        {
            if (point.Time < TimeSpan.Zero)
                point.Time = point.Time.Negate();

            if (point.Time == TimeSpan.Zero && point.TimePointType == TimePointType.Relative)
                point.TimePointType = TimePointType.Absolute;
        }
        private void UpdateTimePointVmCollection (Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?[0] != null) {

                var newTimePoint = (TimePoint) e.NewItems[0];

                PrepareTimePoint(newTimePoint);
                _timePointVmCollection.Add (new TimePointViewModel (newTimePoint, _preset));

                CheckBounds (newTimePoint);
            }

            if (e.OldItems?[0] != null && e.NewItems?[0] == null) {

                var loopNumber = ((TimePoint) e.OldItems[0]).LoopNumber;
                var removingTimePointVm = _timePointVmCollection.First (tpvm => tpvm.TimePoint.Equals ((TimePoint) e.OldItems[0]));

                _timePointVmCollection.Remove (removingTimePointVm);

                if (!_preset.TimerLoops.Keys.Contains (loopNumber)) {

                    var begin = _timePointVmCollection.First (tpvm => tpvm.LoopNumber == loopNumber && tpvm.Id == TimePoint.MinId);
                    _timePointVmCollection.Remove (begin);

                    var end = _timePointVmCollection.First (tpvm => tpvm.LoopNumber == loopNumber && tpvm.Id == TimePoint.MaxId);
                    _timePointVmCollection.Remove (end);
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private void ResetAddingTimePoint()
        {
            AddingTimePoint.Reset();

            OnPropertyChanged (nameof(IsNoTimePoints));
        }

        // AddTimePointCommand:
        private void AddTimePoint(object o)
        {
            _preset.AddTimePoint(_addingTimePoint.TimePoint.Clone());

            ResetAddingTimePoint();
        }
        public bool CanAddTimePoint (object o)
        {
            var res = _addingTimePoint.Time == TimeSpan.Zero && _addingTimePoint.TimePointType == TimePointType.Absolute 
                        || _addingTimePoint.Time > TimeSpan.Zero;

            return res;
        }

        // PlayCommand:
        private void Play(object o) => _timerManager.Play(_preset);
        private bool CanPlay(object o) => TimePointVmCollection.Count > 0;

        // PouseCommand
        private void Pouse (object o) => _timerManager.Pouse();
        private bool CanPouse (object o) => IsRunning;

        // StopCommand
        private void Stop (object o) => _timerManager.Stop();
        private bool CanStop (object o) => IsRunning;

        // StopReset
        private void Resume (object o) => _timerManager.Resume();
        private bool CanResume (object o) => IsRunning;

        // StopReset
        private void ToggleInfinite (object o) => IsInfiniteLoop = (IsInfiniteLoop != true);
        private bool CanToggleInfinite (object o) => !IsRunning;

        // BellOnStartTimeCommand
        private void BellOnStartTime (object o) => CanBellOnStartTime = (CanBellOnStartTime != true);

        // RingCommand
        private void Ring (object o) => TimePointViewModel.DefaultSoundPlayer.Play();

        // TimerManager handlers:
        private void OnSecondPassed(object s, TimerEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnTimePointChanged(object s, TimerEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnStop(object s, EventArgs e)
        {
            throw new NotImplementedException();
        }

        // Checkers:
        private void CheckBounds(TimePoint timePoint)
        {
            if (_settedLoopNumbers.Contains (timePoint.LoopNumber))
                return;

            _timePointVmCollection.Add (new BeginTimePointViewModel (timePoint.LoopNumber, _preset));
            _timePointVmCollection.Add (new EndTimePointViewModel (timePoint.LoopNumber));

            _settedLoopNumbers.Add (timePoint.LoopNumber);
        }

        #endregion
    }
}
