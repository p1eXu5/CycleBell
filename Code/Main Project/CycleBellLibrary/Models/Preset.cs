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
        public static readonly TimeSpan InitDefaultStartTime = TimeSpan.Zero;

        #region Fields

        private TimeSpan? _startTime;
        private readonly ObservableCollection<TimePoint> _timePoints;
        private byte _isInfiniteLoop;

        #endregion

        #region Constructors

        #region Linked Constructors

        public Preset()
           : this(DefaultName, DefaultStartTime)
        { }

        public Preset (string name)
            : this (name, DefaultStartTime)
        { }

        #endregion

        static Preset()
        {
            DefaultStartTime = InitDefaultStartTime;
        }

        private Preset(string name, TimeSpan startTime)
        {
            PresetName = name;

            TimerLoops = new TimerLoopSortedDictionary();
            _timePoints = new ObservableCollection<TimePoint>();

            TimePoints = new ReadOnlyObservableCollection<TimePoint> (_timePoints);

            _startTime = startTime;
        }

        #endregion

        #region Properties

        // Static:
        /// <summary>
        /// Interval in minutes for startTime if it no set
        /// </summary>
        public static TimeSpan DefaultStartTime { get; set; }

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
        /// <exception cref="ArgumentNullException">when timePoint is null</exception>
        public void AddTimePoint(TimePoint timePoint)
        {
            PreAddTimePoint(timePoint);

            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");

            SetBaseTime (timePoint);

            _timePoints.Add(timePoint);

            if (!TimerLoops.ContainsKey(timePoint.LoopNumber)) {
                TimerLoops[timePoint.LoopNumber] = 1;
            }
        }

        public virtual void SetBaseTime (TimePoint timePoint)
        {
            if (TimePoints.Count == 0) {

                timePoint.BaseTime = StartTime;
            }
            else {

                var lastCollectedTimePoint = TimePoints.Where (tp => tp.LoopNumber == timePoint.LoopNumber).OrderBy (tp => tp.Id).Last();
                timePoint.BaseTime = lastCollectedTimePoint.GetAbsoluteTime();
            }
        }

        public virtual void PreAddTimePoint (TimePoint timePoint) { }

        public void RemoveTimePoint(TimePoint timePoint)
        {
            PreRemoveTimePoint (timePoint);

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

        public virtual void PreRemoveTimePoint (TimePoint timePoint) { }

        public void SetInfiniteLoop() => _isInfiniteLoop |= 0x01;
        public void ResetInfiniteLoop() => _isInfiniteLoop ^= _isInfiniteLoop;

        #endregion
    }
}
