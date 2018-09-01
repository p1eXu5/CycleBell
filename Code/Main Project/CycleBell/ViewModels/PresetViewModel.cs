using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

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
        #region Fields

        private readonly Preset _preset;
        private readonly ObservableCollection<TimePointViewModelBase> _timePointVmCollection;

        private TimePointViewModel _addingTimePoint;
        private readonly HashSet<byte> _settedLoopNumbers;

        #endregion

        #region Constructor

        public PresetViewModel(Preset preset)
        {
            _preset = preset ?? throw new ArgumentNullException();
            _settedLoopNumbers = new HashSet<byte>();
            _timePointVmCollection = new ObservableCollection<TimePointViewModelBase>();

            foreach (var point in _preset.TimePoints) {

                PrepareTimePoint(point);
                _timePointVmCollection.Add (new TimePointViewModel (point, _preset));

                CheckBounds(point);
            }

            TimePointVmCollection = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePointVmCollection);

            // TODO CollectionView:
            ICollectionView view = CollectionViewSource.GetDefaultView (TimePointVmCollection);

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add (new SortDescription ("Id", ListSortDirection.Ascending));
            view.SortDescriptions.Add (new SortDescription("LoopNumber", ListSortDirection.Ascending));

            ((INotifyCollectionChanged) _preset.TimePoints).CollectionChanged += UpdateTimePointVmCollection;

            ResetAddingTimePoint();
        }


        #endregion

        #region Properties

        public TimePointViewModel AddingTimePoint
        {
            get => _addingTimePoint;
            set {
                _addingTimePoint = value;
                OnPropertyChanged ();
            }
        }

        public Preset Preset => _preset;
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 5, 0);
        public CycleBellStateFlags State { get; set; }

        public ReadOnlyObservableCollection<TimePointViewModelBase> TimePointVmCollection { get; }
        public TimerLoopSortedDictionary TimerLoops => _preset.TimerLoops;

        #endregion

        #region Commands

        public ICommand AddTimePointCommand => new ActionCommand (AddTimePoint, CanAddTimePoint);

        #endregion

        #region Methods

        private static void PrepareTimePoint (TimePoint point)
        {
            if (point.Time < TimeSpan.Zero)
                point.Time = point.Time.Negate();

            if (point.Time == TimeSpan.Zero && point.TimePointType == TimePointType.Relative)
                point.TimePointType = TimePointType.Absolute;
        }

        private void AddTimePoint(object o)
        {
            _preset.AddTimePoint(_addingTimePoint.TimePoint);

            ResetAddingTimePoint();
        }

        private void CheckBounds(TimePoint timePoint)
        {
            if (_settedLoopNumbers.Contains (timePoint.LoopNumber))
                return;

            _timePointVmCollection.Add (new BeginTimePointViewModel (timePoint.LoopNumber, _preset));
            _timePointVmCollection.Add (new EndTimePointViewModel (timePoint.LoopNumber));

            _settedLoopNumbers.Add (timePoint.LoopNumber);
        }

        private bool CanAddTimePoint(object obj)
        {
            if (_addingTimePoint.Time == TimeSpan.Zero && _addingTimePoint.TimePointType == TimePointType.Absolute 
                || _addingTimePoint.Time > TimeSpan.Zero)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetAddingTimePoint() => AddingTimePoint = new TimePointViewModel (TimePoint.DefaultTimePoint, _preset);

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
        #endregion
    }
}
