using System;
using System.Collections.Generic;
using System.Linq;

namespace CycleBellLibrary
{
    public class Preset
    {
        #region Fields

        private byte _isInfiniteLoop;

        #endregion

        #region Constructors

        #region Linked Constructors

        public Preset()
           : this(DefaultName, null, DefaultInterval, null, false)
        { }

        public Preset(IEnumerable<TimePoint> points)
            : this(DefaultName, points, DefaultInterval, null, false)
        { }

        public Preset(string name)
            : this(name, null, DefaultInterval, null, false)
        { }

        public Preset(string name, int interval)
            : this(name, null, interval, null, false)
        { }

        public Preset(string name, IEnumerable<TimePoint> points)
            : this(name, points, DefaultInterval, null, false)
        { }

        public Preset(string name, TimeSpan startTime)
            : this(name, null, -1, startTime, false)
        { }

        public Preset(string name, IEnumerable<TimePoint> points, TimeSpan startTime)
                : this(name, points, -1, startTime, false)
        { }

        public Preset(string name, IEnumerable<TimePoint> points, TimeSpan startTime, bool isInfinite)
            : this(name, points, -1, startTime, isInfinite)
        { }

        public Preset(string name, IEnumerable<TimePoint> points, string startTime)
            : this(name, points, -1, TimeSpan.Parse(startTime), false)
        { }

        public Preset(string name, IEnumerable<TimePoint> points, string startTime, bool isInfinite)
            : this(name, points, -1, TimeSpan.Parse(startTime), isInfinite)

        { } 
        #endregion

        private Preset(string name, IEnumerable<TimePoint> points, int interval, TimeSpan? startTime, bool isInfinite)
        {
            PresetName = name;

            if (isInfinite) _isInfiniteLoop |= 0x01;

            TimerCycles = new TimerCycleSortedDictionary();
            TimePoints = new List<TimePoint>();

            // TimePoints seting
            var pointList = points?.ToList();

            if (pointList != null && pointList.Count > 0) {

                StartTime = startTime ?? pointList[0].GetAbsoluteTime();

                foreach (var point in pointList) {

                    AddTimePoint(point);
                }

                return;
            }

            // StartTime setting
            var currentTime = DateTime.Now.TimeOfDay;

            if (startTime != null) {
                StartTime = (TimeSpan) startTime;
            }
            else {
                StartTime = new TimeSpan(currentTime.Hours, (currentTime.Minutes + interval), 0);
            }
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
        /// Preset name
        /// </summary>
        public string PresetName { get; set; }

        /// <summary>
        /// Start time for preset
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Indicates whether the loop is infinite
        /// </summary>
        public bool IsInfiniteLoop => _isInfiniteLoop != 0;

        public List<TimePoint> TimePoints { get; }

        /// <summary>
        /// # cycle - n times
        /// </summary>
        public TimerCycleSortedDictionary TimerCycles { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Add NextTimePoint
        /// </summary>
        /// <param name="timePoint"></param>
        public void AddTimePoint(TimePoint timePoint)
        {
            TimePoints.Add(timePoint);

            if (!TimerCycles.ContainsKey(timePoint.TimerCycleNum)) {
                TimerCycles[timePoint.TimerCycleNum] = 1;
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
            if (TimePoints.Contains(timePoint)) {
                TimePoints.Remove(timePoint);
            }

            var points = TimePoints.Where(t => t.TimerCycleNum == timePoint.TimerCycleNum).ToList();

            if (points.Count == 0) {
                TimerCycles.Remove(timePoint.TimerCycleNum);
            }
        }


        public void SetInfiniteLoop() => _isInfiniteLoop |= 0x01;
        public void ResetInfiniteLoop() => _isInfiniteLoop ^= _isInfiniteLoop;

        #endregion
    }
}
