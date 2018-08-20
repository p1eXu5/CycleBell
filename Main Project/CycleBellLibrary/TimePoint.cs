using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CycleBellLibrary
{
    public enum TimePointType : byte
    {
        Relative = 0x01,
        Absolute = 0x02
    }

    [Serializable]
    public class TimePoint
    {
        #region Fields

        private static byte _timePointNum = 0;
        private static int _defaultMinutesToStart = 15;

        private string _name;

        #endregion

        #region Constructors

        public TimePoint()
            : this("Time point " + _timePointNum, TimeSpan.FromMinutes(_defaultMinutesToStart), TimePointType.Relative)
        { }

        public TimePoint(TimeSpan time, TimePointType timePointType = TimePointType.Relative, byte timerCycleNum = 0)
            : this("Time point " + _timePointNum, time, timePointType, timerCycleNum)
        { }

        public TimePoint(string time, TimePointType timePointType = TimePointType.Relative, byte timerCycleNum = 0)
            : this("Time point " + _timePointNum, TimeSpan.Parse(time), timePointType, timerCycleNum)
        { }

        public TimePoint(string name, TimeSpan time, TimePointType timePointType = TimePointType.Relative, byte timerCycleNum = 0)
        {
            LastStartTime = -TimeSpan.FromHours(25);

            Name = name;
            Id = _timePointNum++;

            Time = time;
            TimePointType = timePointType;
            TimerCycleNum = timerCycleNum == Byte.MaxValue ? (byte)(Byte.MaxValue - 1) : timerCycleNum;
        }

        public TimePoint(string name, string time, TimePointType timePointType = TimePointType.Relative, byte timerCycleNum = 0)
            : this(name, TimeSpan.Parse(time), timePointType, timerCycleNum) { }

        #endregion

        #region Properties

        /// <summary>
            /// Дефолтное смещение временной точки относительно текущего времени
            /// </summary>
        public static int DefaultMinutesToStart
        {
            get => _defaultMinutesToStart;
            set => _defaultMinutesToStart = value;
        }

        /// <summary>
        /// Имя при null (для определения StartTime)
        /// </summary>
        public static string FirstPointName { get; set; } = "Launch Time";

        //public static byte LastTimePointNum => (byte)(_timePointNum - 1);

        /// <summary>
        /// Уникальный порядковый номер временной точки
        /// </summary>
        [XmlIgnore]
        public int Id { get; private set; }

        /// <summary>
        /// Name of NextTimePoint
        /// </summary>
        public string Name
        {
            get => _name ?? FirstPointName;
            set => _name = value;
        }

        /// <summary>
        /// Absolute or relative time
        /// </summary>
        public TimeSpan Time { get; set; }

        public TimeSpan LastStartTime { get; set; }

        /// <summary>
        /// Type of Time, absolute or relative
        /// </summary>
        public TimePointType TimePointType { get; set; }

        /// <summary>
        /// Number of queue where this NextTimePoint will measure off
        /// </summary>
        public byte TimerCycleNum { get; set; }

        /// <summary>
        /// Sound file location for example.
        /// </summary>
        public object Tag { get; set; } = null;

        #endregion

        #region Methods

        /// <summary>
        /// Returns absolute time by startTime
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="convertToAbsolute"></param>
        /// <returns></returns>
        public TimeSpan GetAbsoluteTime(TimeSpan startTime, bool convertToAbsolute = false)
        {
            if (TimePointType == TimePointType.Relative) {

                LastStartTime = startTime;

                var res = startTime + Time;

                if (res.Days > 0)
                    res -= TimeSpan.FromDays(1);

                if (convertToAbsolute) {

                    Time = res;
                    TimePointType = TimePointType.Absolute;
                }

                return res;
            }

            return Time;
        }

        public TimeSpan GetAbsoluteTime()
        {
            if (TimePointType == TimePointType.Relative && LastStartTime < TimeSpan.Zero) {
                throw new ArgumentException("LastStartTime must be set");
            }

            return GetAbsoluteTime(LastStartTime);
        }

        public TimeSpan GetRelativeTime(TimeSpan startTime, bool convertToRelative = false)
        {
            LastStartTime = startTime;

            if (TimePointType == TimePointType.Relative || startTime == TimeSpan.Zero) {

                if (TimePointType == TimePointType.Absolute && convertToRelative)
                    TimePointType = TimePointType.Relative;

                return Time;
            }

            TimeSpan res;

            if (Time <= startTime)
                res = TimeSpan.FromHours(24) - startTime + Time;
            else
                res = Time - startTime;

            if (convertToRelative) {

                Time = res;
                TimePointType = TimePointType.Relative;
            }

            return res;
        }

        /// <summary>
        /// Changes Id of current instance of this class with timePoint's Id
        /// </summary>
        /// <param name="timePoint"></param>
        public void ChangeId(TimePoint timePoint)
        {
            timePoint.Id ^= this.Id;
            this.Id ^= timePoint.Id;
            timePoint.Id ^= this.Id;
        }

        public override string ToString() => $"{Name}: {Time:h\\:mm\\:ss} ({TimePointType})"; 

        #endregion
    }
}
