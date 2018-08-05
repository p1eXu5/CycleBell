using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    public class Preset
    {
        #region Fields

        private static int _defaultInterval = 15;

        private readonly ObservableCollection<TimePoint> _timePoints;
        private byte _isInfiniteLoop = 0;

        #endregion

        #region Constructors

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

        private Preset(string name, IEnumerable<TimePoint> points, int interval, TimeSpan? startTime, bool isInfinite)
        {
            PresetName = name;

            if (isInfinite) _isInfiniteLoop |= 0x01;

            TimersCycles = new SortedDictionary<int, int>();
            _timePoints = new ObservableCollection<TimePoint>();

            var pointList = points?.ToList();

            if (pointList != null && pointList.Count > 0) {

                StartTime = startTime ?? pointList[0].GetAbsoluteTime();

                foreach (var point in pointList) {

                    AddTimePoint(point);
                }

                return;
            }

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

        public static int DefaultInterval
        {
            get => _defaultInterval;
            set => _defaultInterval = value;
        }

        public static string DefaultName { get; set; } = "";

        public static Preset EmptyPreset => new Preset();


        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Preset name
        /// </summary>
        public string PresetName { get; set; }

        public IList<TimePoint> TimePoints => _timePoints;

        public SortedDictionary<int, int> TimersCycles { get; }

        public bool IsInfiniteLoop => _isInfiniteLoop != 0;

        #endregion

        #region Methods

        /// <summary>
        /// Add NextTimePoint
        /// </summary>
        /// <param name="timePoint"></param>
        public void AddTimePoint(TimePoint timePoint)
        {
            _timePoints.Add(timePoint);

            if (!TimersCycles.ContainsKey(timePoint.TimerCycleNum)) {
                TimersCycles[timePoint.TimerCycleNum] = 1;
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
            };

            var points = _timePoints.Where(t => t.TimerCycleNum == timePoint.TimerCycleNum).ToList();

            if (points.Count == 0) {
                TimersCycles.Remove(timePoint.TimerCycleNum);
            }
        }


        public void SetInfiniteLoop() => _isInfiniteLoop |= 0x01;
        public void ResetInfiniteLoop() => _isInfiniteLoop ^= _isInfiniteLoop;

        #endregion

    }
}
