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

        #endregion

        #region Constructor

        public PresetViewModel(Preset preset)
        {
            _preset = preset;

            _timePoints = new ObservableCollection<TimePointViewModelBase>(_preset.TimePoints.Select(t => new TimePointViewModel(t)));
            TimePoints = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePoints);

            ((INotifyCollectionChanged) _preset.TimePoints).CollectionChanged += (s, e) =>
            {
                if (e.NewItems[0] != null)
                    _timePoints.Add (new TimePointViewModel ((TimePoint)e.NewItems[0]));
            };

            _addingTimePoint = new TimePointViewModel (TimePoint.DefaultTimePoint);
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

        public ICommand AddTimePointCommand => new ActionCommand (o => AddTimePoint(), CanAddTimePoint);

        #endregion

        #region Methods

        private void AddTimePoint()
        {
            _preset.AddTimePoint(_addingTimePoint.TimePoint);
            AddingTimePoint = new TimePointViewModel(TimePoint.DefaultTimePoint);
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
