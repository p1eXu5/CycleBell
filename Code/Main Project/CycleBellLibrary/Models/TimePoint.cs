using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CycleBellLibrary.Models
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

        #region Constructor Overrides

        public TimePoint()
            : this("Time point " + _timePointNum, TimeSpan.FromMinutes(_defaultMinutesToStart), TimePointType.Relative)
        { }

        public TimePoint(TimeSpan time, TimePointType timePointType = TimePointType.Relative, byte timerCycleNum = 0)
            : this("Time point " + _timePointNum, time, timePointType, timerCycleNum)
        { }

        public TimePoint(string time, TimePointType timePointType = TimePointType.Relative, byte timerCycleNum = 0)
            : this("Time point " + _timePointNum, TimeSpan.Parse(time), timePointType, timerCycleNum)
        { }

        public TimePoint(string name, string time, TimePointType timePointType = TimePointType.Relative, byte loopNumber = 0)
            : this(name, TimeSpan.Parse(time), timePointType, loopNumber) { }

        #endregion

        /// <summary>
        /// Creates TimePoint instance
        /// </summary>
        /// <param name="name">Name of TimePoint</param>
        /// <param name="time">An absolute or relative time of day</param>
        /// <param name="timePointType">A type of TimePoint</param>
        /// <param name="loopNumber">The loop number</param>
        public TimePoint(string name, TimeSpan time, TimePointType timePointType = TimePointType.Relative, byte loopNumber = 0)
        {
            if (_timePointNum == byte.MaxValue)
                throw new OverflowException ("Reached the maximum number of TimePoints");

            Name = name;
            Id = _timePointNum++;

            Time = time;
            TimePointType = timePointType;
            LoopNumber = loopNumber == Byte.MaxValue ? (byte)(Byte.MaxValue - 1) : loopNumber;
        }

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
        /// </summary>-
        public static string FirstPointName { get; set; } = "Launch Time";

        public static int MaxId => Int32.MaxValue;
        public static int MinId => Int32.MinValue;

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

        public TimeSpan? BaseTime { get; set; } = null;

        /// <summary>
        /// Type of Time, absolute or relative
        /// </summary>
        public TimePointType TimePointType { get; set; }

        /// <summary>
        /// Number of queue where this NextTimePoint will measure off
        /// </summary>
        public byte LoopNumber { get; set; }

        /// <summary>
        /// Sound file location for example.
        /// </summary>
        public object Tag { get; set; } = null;

        #endregion

        #region Methods

        /// <summary>
        /// Returns absolute time by baseTime
        /// </summary>
        /// <param name="baseTime"></param>
        /// <param name="convertToAbsolute"></param>
        /// <returns></returns>
        public TimeSpan GetAbsoluteTime(TimeSpan baseTime)
        {
            if (TimePointType == TimePointType.Relative && baseTime != TimeSpan.Zero) {

                BaseTime = baseTime;

                var res = baseTime + Time;

                if (res.Days > 0)
                    res -= TimeSpan.FromDays(1);

                return res;
            }

            return Time;
        }

        /// <summary>
        /// Gets absolute time
        /// </summary>
        /// <returns>Absolute time</returns>
        /// <exception cref="ArgumentException"></exception>
        public TimeSpan GetAbsoluteTime()
        {
            if (TimePointType == TimePointType.Absolute)
                return Time;

            if (!BaseTime.HasValue) {
                throw new ArgumentException("BaseTime must be set");
            }

            return GetAbsoluteTime((TimeSpan)BaseTime);
        }

        public TimeSpan GetRelativeTime()
        {
            if (TimePointType == TimePointType.Relative)
                return Time;

            if (!BaseTime.HasValue) {
                throw new ArgumentException("BaseTime must be set");
            }

            return GetRelativeTime((TimeSpan)BaseTime);
        }

        public TimeSpan GetRelativeTime(TimeSpan baseTime)
        {
            BaseTime = baseTime;

            if (TimePointType == TimePointType.Relative || baseTime == TimeSpan.Zero) {

                return Time;
            }

            TimeSpan res;

            if (Time <= baseTime)
                res = TimeSpan.FromHours(24) - baseTime + Time;
            else
                res = Time - baseTime;

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
