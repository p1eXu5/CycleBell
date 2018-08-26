using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CycleBellLibrary
{
    /// <summary>
    /// If StartTime not set, getter returns negative TimeSpan (TimeSpan.FromSecond(-1))
    /// </summary>
    public class Preset
    {
        #region Fields

        private TimeSpan? _startTime = null;
        private ObservableCollection<TimePoint> _timePoints;
        private byte _isInfiniteLoop;

        #endregion

        #region Constructors

        #region Linked Constructors

        public Preset()
           : this(DefaultName, null, DefaultInterval)
        { }

        public Preset(IEnumerable<TimePoint> points)
            : this(DefaultName, points, DefaultInterval)
        { }

        #endregion

        private Preset(string name, IEnumerable<TimePoint> points, int interval)
        {
            PresetName = name;

            TimerLoops = new TimerLoopSortedDictionary();
            _timePoints = new ObservableCollection<TimePoint>();

            // TimePoints seting
            var pointList = points?.ToList();

            if (pointList != null && pointList.Count > 0) {


                foreach (var point in pointList) {

                    AddTimePoint(point);
                }

                return;
            }

            TimePoints = new ReadOnlyObservableCollection<TimePoint> (_timePoints);

        }

        #endregion

        #region Properties

        /// <summary>
        /// Interval in minutes for startTime if it no set
        /// </summary>
        public static int DefaultInterval { get; set; } = 15;

        /// <summary>
        /// Default name for presets
        /// </summary>
        public static string DefaultName { get; set; } = "";

        /// <summary>
        /// Returns Preset with default settings
        /// </summary>
        public static Preset EmptyPreset => new Preset();

        /// <summary>
        /// Start TimePoint name
        /// </summary>
        public string StartTimePointName { get; set; } = "Start Time Point";

        /// <summary>
        /// Preset name
        /// </summary>
        public string PresetName { get; set; }

        /// <summary>
        /// Start time for preset
        /// </summary>
        public TimeSpan StartTime
        {
            get => _startTime ?? TimeSpan.FromSeconds(-1); 
            set => _startTime = value;
        }

        /// <summary>
        /// Indicates whether the loop is infinite
        /// </summary>
        public bool IsInfiniteLoop => _isInfiniteLoop != 0;

        public ReadOnlyObservableCollection<TimePoint> TimePoints { get; }

        /// <summary>
        /// # cycle - n times
        /// </summary>
        public TimerLoopSortedDictionary TimerLoops { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Add NextTimePoint
        /// </summary>
        /// <param name="timePoint"></param>
        public void AddTimePoint(TimePoint timePoint)
        {
            _timePoints.Add(timePoint);

            if (!TimerLoops.ContainsKey(timePoint.LoopNumber)) {
                TimerLoops[timePoint.LoopNumber] = 1;
            }
        }

        public void AddTimePointRange(IEnumerable<TimePoint> timePoints)
        {
            foreach (TimePoint timePoint in timePoints) {
                AddTimePoint(timePoint);
            }
        }

        public void RemoveTimePoint(TimePoint timePoint)
        {
            if (_timePoints.Contains(timePoint)) {
                _timePoints.Remove(timePoint);
            }

            var points = TimePoints.Where(t => t.LoopNumber == timePoint.LoopNumber).ToList();

            if (points.Count == 0) {
                TimerLoops.Remove(timePoint.LoopNumber);
            }
        }


        public void SetInfiniteLoop() => _isInfiniteLoop |= 0x01;
        public void ResetInfiniteLoop() => _isInfiniteLoop ^= _isInfiniteLoop;

        #endregion
    }
}
