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
        private readonly IPresetCollectionManager _presetCollectionManager;

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

            // _presetCollectionManager
            _presetCollectionManager = manager.PresetCollectionManager ?? throw new ArgumentNullException(nameof(PresetCollectionManager));

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
            _name = new StringBuilder(GetPresetName(_preset.PresetName));

            // AddingTimePoint
            AddingTimePoint = GetAddingTimePointViewModel(_preset);
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
        
        public ICommand BellOnStartTimeToggleCommand => new ActionCommand (BellOnStartTime);


        #endregion

        #region Methods

        public void Save() { throw new NotImplementedException(); }

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

        private AddingTimePointViewModel GetAddingTimePointViewModel (Preset preset)
        {
            var timePoint = TimePoint.DefaultTimePoint;
            timePoint.Name = "";
            var addingTimePoint = new AddingTimePointViewModel (preset);
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

        private void UpdateTimePointVmCollection (Object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add
            if (e.NewItems?[0] != null) {

                var newTimePoint = (TimePoint) e.NewItems[0];

                PrepareTimePoint(newTimePoint);
                _timePointVmCollection.Add (new TimePointViewModel (newTimePoint, _preset));

                CheckBounds (newTimePoint);

                return;
            }

            // Remove
            if (e.OldItems?[0] != null && e.NewItems?[0] == null) {

                var loopNumber = ((TimePoint) e.OldItems[0]).LoopNumber;
                var timePoints = _timePointVmCollection.Where (tpvm => tpvm.LoopNumber == loopNumber).ToArray();

                if (timePoints.Length == 3) {

                    _timePointVmCollection.Clear();
                    return;
                }

                var removingTimePointVm = timePoints.First (tpvm => tpvm.TimePoint.Equals ((TimePoint) e.OldItems[0]));
                _timePointVmCollection.Remove (removingTimePointVm);
            }
        }
     
        public PresetViewModel GetDeepCopy()
        {
            var presetVm = (PresetViewModel) this.MemberwiseClone();
            //presetVm._preset = _preset.GetDeepCopy();

            return null;
        }

        // AddTimePointCommand:
        private void AddTimePoint(object o)
        {
            var timePoint = _addingTimePoint.TimePoint.Clone();

            if (String.IsNullOrWhiteSpace (timePoint.Name))
                timePoint.Name = timePoint.GetDefaultTimePointName();

            _preset.AddTimePoint(timePoint);

            ResetAddingTimePoint();
        }
        public bool CanAddTimePoint (object o)
        {
            var res = _addingTimePoint.Time == TimeSpan.Zero && _addingTimePoint.TimePointType == TimePointType.Absolute 
                        || _addingTimePoint.Time > TimeSpan.Zero;

            return res;
        }



        // BellOnStartTimeCommand
        private void BellOnStartTime (object o) => CanBellOnStartTime = (CanBellOnStartTime != true);



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

        /// <summary>
        /// Used in UpdateTimePointVmCollection
        /// </summary>
        /// <param name="timePoint"></param>
        private void CheckBounds(TimePoint timePoint)
        {
            if (_settedLoopNumbers.Contains (timePoint.LoopNumber))
                return;

            _timePointVmCollection.Add (new BeginTimePointViewModel (timePoint.LoopNumber, _preset));
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

            if (point.Time == TimeSpan.Zero && point.TimePointType == TimePointType.Relative)
                point.TimePointType = TimePointType.Absolute;
        }
        
        #endregion
    }
}
