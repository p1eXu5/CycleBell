using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

        private TimeSpan _startTime;
        private ObservableCollection<TimePoint> _timePoints;
        private ReadOnlyObservableCollection<TimePoint> _readOnlyTimePointCollection;
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

        public Preset (IEnumerable<TimePoint> timePoints)
            : this (DefaultName, DefaultStartTime)
        {
            if (timePoints == null)
                throw new ArgumentNullException();

            var autoUpdt = AutoUpdateTimePointBaseTimes;

            if (AutoUpdateTimePointBaseTimes)
                AutoUpdateTimePointBaseTimes = false;

            foreach (var timePoint in timePoints) {
                AddTimePoint (timePoint);
            }

            AutoUpdateTimePointBaseTimes = autoUpdt;
        }

        #endregion

        // Static
        static Preset()
        {
            DefaultStartTime = InitDefaultStartTime;
        }

        // Instance
        private Preset(string name, TimeSpan startTime)
        {
            PresetName = name;

            TimerLoops = new TimerLoopSortedDictionary();
            _timePoints = new ObservableCollection<TimePoint>();

            _readOnlyTimePointCollection = new ReadOnlyObservableCollection<TimePoint> (_timePoints);

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

        /// <summary>
        /// Start TimePoint name
        /// </summary>
        public static string StartTimePointName { get; set; } = "Start Time Point";

        /// <summary>
        /// When true TimePoint BaseTimes updates automaticaly after adding TimePoint or setting StartTime.
        /// </summary>
        public static bool AutoUpdateTimePointBaseTimes { get; set; } = true;

        // Instance:
        /// <summary>
        /// Preset name
        /// </summary>
        public string PresetName { get; set; }

        /// <summary>
        /// Start time for preset
        /// </summary>
        public TimeSpan StartTime
        {
            get => _startTime;
            set => _startTime = AutoUpdateTimePointBaseTimes ? OnStartTimeChanged (value, _startTime) : value;
        }

        /// <summary>
        /// Indicates whether the loop is infinite
        /// </summary>
        public bool IsInfiniteLoop => _isInfiniteLoop != 0;

        public ReadOnlyObservableCollection<TimePoint> TimePoints => _readOnlyTimePointCollection;

        public virtual IEnumerable<TimePoint> GetOrderedTimePoints(byte ln) => _timePoints.OrderBy (tp => tp.Id).Where (tp => tp.LoopNumber == ln);
        public virtual IEnumerable<TimePoint> GetOrderedTimePoints() => _timePoints.OrderBy (tp => tp.Id).ThenBy (tp => tp.LoopNumber);

        /// <summary>
        /// # cycle - n times
        /// </summary>
        public TimerLoopSortedDictionary TimerLoops { get; set; }

        public Object Tag { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Add NextTimePoint
        /// </summary>
        /// <param name="timePoint"></param>
        /// <exception cref="ArgumentNullException">when timePoint is null</exception>
        public void AddTimePoint(TimePoint timePoint)
        {
            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");
         
            if (TimePoints.Contains (timePoint))
                throw new ArgumentException("timePoint already exists", nameof(timePoint));

            PreAddTimePoint(timePoint);

            if (TimePoints.Count > 0)
                PrepareTimePointId (timePoint);

            _timePoints.Add(timePoint);

            AddLoopNumber(timePoint);

            timePoint.CollectionChanged += OnTimePointLoopNumberChanged;

            if (AutoUpdateTimePointBaseTimes)
                UpdateTimePointBaseTimes();
        }
        public virtual void PreAddTimePoint (TimePoint timePoint) { timePoint.BaseTime = null; }
        
        public void RemoveTimePoint(TimePoint timePoint)
        {
            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");

            if (!_timePoints.Contains (timePoint))
                throw new ArgumentException("timePoint not in collection", nameof(timePoint));

            PreRemoveTimePoint (timePoint);

            if (_timePoints.Contains(timePoint)) {
                _timePoints.Remove(timePoint);
            }

            if (TimePoints.FirstOrDefault (t => t.LoopNumber == timePoint.LoopNumber) == null) {

                TimerLoops.Remove(timePoint.LoopNumber);
            }

            if (AutoUpdateTimePointBaseTimes)
                UpdateTimePointBaseTimes();
        }
        public virtual void PreRemoveTimePoint (TimePoint timePoint) { }
        
        public void SetInfiniteLoop() => _isInfiniteLoop |= 0x01;
        public void ResetInfiniteLoop() => _isInfiniteLoop ^= _isInfiniteLoop;

        public Preset GetDeepCopy()
        {
            var timePoints = _timePoints;
            _timePoints = new ObservableCollection<TimePoint>(_timePoints);
            _readOnlyTimePointCollection = new ReadOnlyObservableCollection<TimePoint>(_timePoints);

            var timerLoops = TimerLoops;
            TimerLoops = new TimerLoopSortedDictionary();
            foreach (var key in timerLoops.Keys) {
                TimerLoops[key] = timerLoops[key];
            }

            var tag = Tag;
            Tag = new object();

            var preset = (Preset)this.MemberwiseClone();

            Tag = tag;

            TimerLoops = timerLoops;

            _timePoints = timePoints;
            _readOnlyTimePointCollection = new ReadOnlyObservableCollection<TimePoint>(_timePoints);

            return preset;
        }

        private void AddLoopNumber (TimePoint timePoint)
        {
            if (!TimerLoops.ContainsKey (timePoint.LoopNumber)) {
                TimerLoops[timePoint.LoopNumber] = 1;
            }
        }

        private void PrepareTimePointId (TimePoint timePoint)
        {
            var maxId = TimePoints.Max (tp => tp.Id);

            if (timePoint.Id <= maxId) {

                var tmp = TimePoint.DefaultTimePoint;
                timePoint.ChangeId (tmp);
            }
        }

        /// <summary>
        /// Calls when TimePoint LoopNumber changed
        /// </summary>
        /// <param name="sender">TimePoint</param>
        /// <param name="args">The new and old LoopNumbers</param>
        private void OnTimePointLoopNumberChanged (object sender, NotifyCollectionChangedEventArgs args)
        {
            if (sender is TimePoint tp) {
                
                AddLoopNumber (tp);

                if (TimePoints.FirstOrDefault (t => t.LoopNumber == (Byte) args.OldItems[0]) == null) {

                    this.TimerLoops.Remove ((Byte) args.OldItems[0]);
                }
            }

            if (AutoUpdateTimePointBaseTimes)
                UpdateTimePointBaseTimes();
        }

        /// <summary>
        /// Calls when StartTimeChanged.
        /// </summary>
        /// <param name="newStartTime"></param>
        /// <param name="oldStartTime"></param>
        /// <returns>Returns newStartTime.</returns>
        private TimeSpan OnStartTimeChanged(TimeSpan newStartTime, TimeSpan oldStartTime)
        {
            if (TimePoints.Count == 0)
                goto exit;

            var diff = newStartTime - oldStartTime;

            foreach (var timePoint in TimePoints) {

                if (timePoint.TimePointType == TimePointType.Relative) {
                    timePoint.BaseTime += diff;
                }
                else {
                    timePoint.BaseTime += diff;
                    timePoint.Time += diff;
                }
            }

            exit:
            return newStartTime;
        }

        /// <summary>
        /// Updates TimePoint BaseTimes
        /// </summary>
        protected void UpdateTimePointBaseTimes()
        {
            var array = GetOrderedTimePoints().ToArray();

            if (array[0].TimePointType == TimePointType.Relative) {
                array[0].BaseTime = StartTime;
            }

            for (int i = 1; i < array.Length; ++i) {

                if (array[i].TimePointType == TimePointType.Relative) {
                    array[i].BaseTime = array[i - 1].GetAbsoluteTime();
                }
            }
        }

        #endregion
    }
}
