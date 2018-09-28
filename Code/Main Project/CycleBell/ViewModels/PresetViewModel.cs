using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
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
    public class PresetViewModel : ObservableObject, IPresetViewModel
    {
        #region Consts

        private const string StarString = "*";
        private const string NewString = "new";

        #endregion

        #region Fields

        private readonly Preset _preset;
        private readonly IMainViewModel _mainViewModel;

        private readonly ObservableCollection<TimePointViewModelBase> _timePointVmCollection;
        private TimePointViewModelBase _selectedTimePoint;
        private AddingTimePointViewModel _addingTimePoint;

        private bool _isModified;
        private bool _canBellOnStartTime;

        private readonly HashSet<byte> _settedLoopNumbers;

        private StringBuilder _name;

        private string _nextTimePointName;
        private TimeSpan _timeLeftTo;

        #endregion

        #region Constructor

        public PresetViewModel(Preset preset, IMainViewModel mainViewModel)
        {
            // _PresetViewModel
            _preset = preset ?? throw new ArgumentNullException(nameof(preset));

            // _presetCollectionManager
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // _settedLoopNumbers
            _settedLoopNumbers = new HashSet<byte>();

            // _timePointVmCollection
            _timePointVmCollection = new ObservableCollection<TimePointViewModelBase>();
            LoadTimePointViewModelCollection(_preset);
            
            // TimePointVmCollection
            TimePointVmCollection = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePointVmCollection);

            // CollectionView:
            ICollectionView view = CollectionViewSource.GetDefaultView (TimePointVmCollection);

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add (new SortDescription ("Id", ListSortDirection.Ascending));
            view.SortDescriptions.Add (new SortDescription("LoopNumber", ListSortDirection.Ascending));

            // TimePointCollection INotifyCollectionChanged
            ((INotifyCollectionChanged) _preset.TimePointCollection).CollectionChanged += OnTimePointCollectionChanged;

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
            AddingTimePoint = GetAddingTimePointViewModel();
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

        public string NextTimePointName
        {
            get => _nextTimePointName;
            set {
                _nextTimePointName = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan TimeLeftTo
        {
            get => _timeLeftTo;
            set {
                _timeLeftTo = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand AddTimePointCommand => new ActionCommand (AddTimePoint, CanAddTimePoint);

        #endregion

        #region Methods

        public void RemoveTimePoint (TimePoint timePoint)
        {
            _preset.RemoveTimePoint (timePoint);
        }

        public void UpdateSoundBank (TimePoint timePoint)
        {
            _mainViewModel.SoundMap[timePoint.Id] = new SoundPlayer((string)timePoint.Tag);
        }

        public void Save() { throw new NotImplementedException(); }

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

        private void OnTimePointCollectionChanged (Object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add
            if (e.NewItems?[0] is TimePoint newTimePoint) {

                //var newTimePoint = (TimePoint) e.NewItems[0];

                PrepareTimePoint(newTimePoint);
                _timePointVmCollection.Add (new TimePointViewModel (newTimePoint, this));

                CheckBounds (newTimePoint);

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
                    return;
                }

                var removingTimePointVm = timePoints.First (tpvm => tpvm.Id == oldTimePoint.Id);
                _timePointVmCollection.Remove (removingTimePointVm);
            }
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
        private bool CanAddTimePoint (object o)
        {
            var res = _addingTimePoint.Time == TimeSpan.Zero && _addingTimePoint.TimePointType == TimePointType.Absolute 
                        || _addingTimePoint.Time > TimeSpan.Zero;

            return res;
        }
        private bool CanAddTimePoint (TimePoint timePoint)
        {
            var res = timePoint.Time == TimeSpan.Zero && timePoint.TimePointType == TimePointType.Absolute 
                      || timePoint.Time > TimeSpan.Zero;

            return res;
        }

        // TimerManager handlers:
        internal void OnTimePointChangedEventHandler(object s, TimerEventArgs e)
        {
            // if prev TimePoint it is launch TimePoint
            if (e.PrevTimePoint.Time < TimeSpan.Zero) {

                NextTimePointName = _mainViewModel.StartTimeName;
            }
            else {
                TimePointVmCollection.Diactivate().Activate(tpvmb => tpvmb == e.PrevTimePoint);
            }

            TimeLeftTo = e.LastTime;
        }

        internal void OnSecondPassedEventHandler(object s, TimerEventArgs e)
        {
            TimeLeftTo = e.LastTime;
        }

        internal void OnStopEventHandler(object s, EventArgs e)
        {
            throw new NotImplementedException();
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

            if (point.Time == TimeSpan.Zero && point.TimePointType == TimePointType.Relative)
                point.TimePointType = TimePointType.Absolute;
        }
        
        #endregion
    }

    /// <summary>
    /// Extension class for ReadOnlyObservableCollection&lt;TimePointViewModel&gt;
    /// </summary>
    internal static class TimePointViewModelExtension
    {
        internal static ReadOnlyObservableCollection<TimePointViewModelBase> Diactivate(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels)
        {
            if (timePointViewModels == null)
                return null;

            var tpvmArray = timePointViewModels.Where(t => (t is TimePointViewModel model) && model.Active).ToArray();

            foreach (var timePointViewModel in tpvmArray) {

                ((TimePointViewModel)timePointViewModel).Active = false;
            }

            return timePointViewModels;
        }

        internal static void Activate(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels, Func<TimePointViewModelBase,bool> predicate)
        {
            var tpvm = timePointViewModels?.Where(predicate).FirstOrDefault();

            if (tpvm == null)
                return;

            ((TimePointViewModel)tpvm).Active = true;
        }
    }
}
