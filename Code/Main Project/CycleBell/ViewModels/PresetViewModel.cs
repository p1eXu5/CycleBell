using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CycleBell.Base;
using CycleBellLibrary;
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
        private readonly Preset _preset;
        private readonly ObservableCollection<TimePointViewModelBase> _timePoints;

        #region Constructor

        public PresetViewModel(Preset preset)
        {
            _preset = preset;

            _timePoints = new ObservableCollection<TimePointViewModelBase>(_preset.TimePoints.Select(t => new TimePointViewModel(t)));
            TimePoints = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePoints);

            ((INotifyCollectionChanged) _preset.TimePoints).CollectionChanged += (s, e) => { };
        }

        #endregion

        public string Name { get; set; }
        public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 5, 0);
        public CycleBellStateFlags State { get; set; }

        public ReadOnlyObservableCollection<TimePointViewModelBase> TimePoints { get; }
        public TimerLoopSortedDictionary TimerLoops => _preset.TimerLoops;
    }
}
