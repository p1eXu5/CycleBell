using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Repository
{
    /// <summary>
    /// It's a TimePoint repository. If StartTime not set, getter returns negative TimeSpan (TimeSpan.FromSecond(-1))
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
           : this(DefaultName, null)
        { }

        public Preset (string presetName)
            : this (presetName, null)
        { }

        public Preset(IEnumerable<TimePoint> points)
            : this(DefaultName, points)
        { }

        #endregion

        private Preset(string name, IEnumerable<TimePoint> points)
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

        // Static:
        /// <summary>
        /// Interval in minutes for startTime if it no set
        /// </summary>
        public static TimeSpan DefaultStartTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Default name for presets
        /// </summary>
        public static string DefaultName { get; set; } = "";

        /// <summary>
        /// Returns Preset with default settings
        /// </summary>
        public static Preset EmptyPreset => new Preset();

        // Public:
        /// <summary>
        /// Algorythms for TimePoints BaseTime set
        /// </summary>
        public ITimePointBaseTimeSetter BaseTimeSetter { get; set; }

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

        public virtual ReadOnlyObservableCollection<TimePoint> TimePoints { get; }

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
        public virtual void AddTimePoint(TimePoint timePoint)
        {
            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");

            _timePoints.Add(timePoint);

            if (!TimerLoops.ContainsKey(timePoint.LoopNumber)) {
                TimerLoops[timePoint.LoopNumber] = 1;
            }
        }

        public void AddTimePointRange(IEnumerable<TimePoint> timePoints)
        {
            if (timePoints == null)
                throw new ArgumentNullException();

            foreach (TimePoint timePoint in timePoints) {
                AddTimePoint(timePoint);
            }
        }

        public virtual void RemoveTimePoint(TimePoint timePoint)
        {
            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");

            if (!_timePoints.Contains (timePoint))
                throw new ArgumentException("timePoint not in collection", nameof(timePoint));

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
