using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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
        private readonly ObservableCollection<TimePointViewModelBase> _timePoints;

        private TimePointViewModel _addingTimePoint;
        private readonly HashSet<byte> _settedLoopNumbers;

        #endregion

        #region Constructor

        public PresetViewModel(Preset preset)
        {
            _preset = preset ?? throw new ArgumentNullException();
            _settedLoopNumbers = new HashSet<byte>();
            _timePoints = new ObservableCollection<TimePointViewModelBase>();

            foreach (var point in _preset.TimePoints) {

                if (point.Time < TimeSpan.Zero)
                    point.Time = point.Time.Negate();

                if (point.Time == TimeSpan.Zero && point.TimePointType == TimePointType.Relative)
                    point.TimePointType = TimePointType.Absolute;

                _timePoints.Add (new TimePointViewModel (point));

                CheckBounds(point);
            }

            TimePoints = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePoints);

            ((INotifyCollectionChanged) _preset.TimePoints).CollectionChanged += (s, e) =>
            {
                if (e.NewItems[0] != null)
                    _timePoints.Add (new TimePointViewModel ((TimePoint)e.NewItems[0]));
            };

            AddingTimePoint = new TimePointViewModel (TimePoint.DefaultTimePoint);
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

        public string Name { get; set; }
        public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 5, 0);
        public CycleBellStateFlags State { get; set; }

        public ReadOnlyObservableCollection<TimePointViewModelBase> TimePoints { get; }
        public TimerLoopSortedDictionary TimerLoops => _preset.TimerLoops;

        #endregion

        #region Commands

        public ICommand AddTimePointCommand => new ActionCommand (AddTimePoint, CanAddTimePoint);

        #endregion

        #region Methods

        private void AddTimePoint(object o)
        {
            // TODO:
            _preset.AddTimePoint(_addingTimePoint.TimePoint);
            CheckBounds (_addingTimePoint.TimePoint);

            // Resetting AddingTimePoint
            AddingTimePoint = new TimePointViewModel(TimePoint.DefaultTimePoint);
        }

        private void CheckBounds(TimePoint timePoint)
        {
            if (_settedLoopNumbers.Contains (timePoint.LoopNumber))
                return;

            _timePoints.Add (new BeginTimePointViewModel (timePoint.LoopNumber, this));
            _timePoints.Add (new EndTimePointViewModel (timePoint.LoopNumber));

            _settedLoopNumbers.Add (timePoint.LoopNumber);
        }

        private bool CanAddTimePoint(object obj)
        {
            if (_addingTimePoint.Time == TimeSpan.Zero && _addingTimePoint.TimePointType == TimePointType.Absolute 
                || _addingTimePoint.Time > TimeSpan.Zero)
                return true;

            return false;
        }

        #endregion
    }
}
