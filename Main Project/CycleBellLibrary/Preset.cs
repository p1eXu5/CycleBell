using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CycleBellLibrary
{
    [Serializable]
    [XmlRoot(Namespace = "http://www.MyCompany.com")]
    public class Preset
    {
        #region Fields

        private readonly ObservableCollection<TimePoint> _timePoints;
        private byte _isInfiniteLoop = 0;

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

        /// <summary>
        /// Interval in minutes for startTime if it no set
        /// </summary>
        [XmlIgnore]
        public static int DefaultInterval { get; set; } = 15;

        /// <summary>
        /// Default name for presets
        /// </summary>
        [XmlIgnore]
        public static string DefaultName { get; set; } = "";

        /// <summary>
        /// Returns Preset with default settings
        /// </summary>
        [XmlIgnore]
        public static Preset EmptyPreset => new Preset();

        /// <summary>
        /// Preset name
        /// </summary>
        [XmlElement(Order = 1)]
        public string PresetName { get; set; }

        /// <summary>
        /// Start time for preset
        /// </summary>
        [XmlElement(Order = 2)]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Indicates whether the loop is infinite
        /// </summary>
        [XmlElement(Order = 3)]
        public bool IsInfiniteLoop => _isInfiniteLoop != 0;

        public IList<TimePoint> TimePoints => _timePoints;

        /// <summary>
        /// # cycle - n times
        /// </summary>
        [XmlIgnore]
        public SortedDictionary<int, int> TimersCycles { get; }


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
