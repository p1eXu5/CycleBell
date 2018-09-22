using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CycleBellLibrary.Annotations;

namespace CycleBellLibrary.Models
{
    public enum TimePointType : byte
    {
        Relative = 0x01,
        Absolute = 0x02
    }

    [Serializable]
    public class TimePoint : INotifyCollectionChanged
    {
        #region Fields

        public static readonly TimeSpan InitialDefaultTime = TimeSpan.Zero;

        private static byte _timePointNum = Byte.MinValue + 1;

        private string _name;
        private TimeSpan _time;
        private byte _loopNumber;

        #endregion

        #region Constructors

        // Static!
        static TimePoint()
        {
            DefaultTime = InitialDefaultTime;

            // Да хуй его, прикольно было попробовать
            Type type = MaxId.GetType();

            dynamic max = _timePointNum.GetType().GetField ("MaxValue")?.GetValue (null);

            if (max == null)
                throw new InvalidOperationException();

            MaxId = (Int64) max;

            dynamic min = _timePointNum.GetType().GetField ("MinValue")?.GetValue (null);

            if (min == null)
                throw new InvalidOperationException();

            MinId = (Int64) min;

        }

        // Instance
        public TimePoint()
            : this(
                name: DefaultTimePointNameFunc(), 
                time: DefaultTime, 
                timePointType: DefaultTimePointType)
        { }
        
        /// <summary>
        /// Creates TimePoint instance
        /// </summary>
        /// <param name="name">Name of TimePoint</param>
        /// <param name="time">An absolute or relative time of day</param>
        /// <param name="timePointType">A type of TimePoint</param>
        /// <param name="loopNumber">The loop number</param>
        public TimePoint(string name, TimeSpan time, TimePointType timePointType, byte loopNumber = 0)
        {
            if (_timePointNum == MaxId)
                throw new OverflowException ("Reached the maximum number of TimePoints");

            Name = name;
            Id = _timePointNum++;

            Time = time;
            TimePointType = timePointType;

            if (timePointType == TimePointType.Absolute)
                BaseTime = TimeSpan.Zero;

            LoopNumber = loopNumber == Byte.MaxValue ? (byte)(Byte.MaxValue - 1) : loopNumber;
        }

        #region Linked

        public TimePoint(TimeSpan time, TimePointType timePointType)
            : this("Time point " + _timePointNum, time, timePointType)
        { }
        public TimePoint(string time, TimePointType timePointType)
            : this("Time point " + _timePointNum, TimeSpan.Parse(time), timePointType)
        { }
        public TimePoint(string time)
            :this("Time point " + _timePointNum, TimeSpan.Parse(time), DefaultTimePointType)
        { }
        public TimePoint(string name, string time)
            :this(name, TimeSpan.Parse(time), DefaultTimePointType)
        { }
        public TimePoint(string name, string time, TimePointType timePointType)
            :this(name, TimeSpan.Parse(time), timePointType)
        { }

        #endregion

        #endregion

        #region Properties

        // Static:_________________________________________________________________________

        /// <summary>
            /// Дефолтное смещение временной точки относительно текущего времени
            /// </summary>
        public static TimeSpan DefaultTime { get; set; }

        public static TimePointType DefaultTimePointType { get; set; } = TimePointType.Relative;

        /// <summary>
        /// Имя при null (для определения StartTime)
        /// </summary>-
        public static string FirstPointName { get; set; } = "Launch Time";

        public static Int64 MaxId { get; }
        public static Int64 MinId { get; }

        public static TimePoint DefaultTimePoint => new TimePoint();

        public static Func<String> DefaultTimePointNameFunc = () => $"TimePoint {_timePointNum}";

        // Instance:_______________________________________________________________________

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
        public TimeSpan Time
        {
            get => _time; 
            set { _time = value; }
        }

        public TimeSpan? BaseTime { get; set; } = null;

        /// <summary>
        /// Type of Time, absolute or relative
        /// </summary>
        public TimePointType TimePointType { get; set; }

        /// <summary>
        /// Number of queue where this NextTimePoint will measure off
        /// </summary>
        public byte LoopNumber 
        { 
            get => _loopNumber;
            set {
                byte oldValue = _loopNumber;
                _loopNumber = value;
                OnCollectionChanged (_loopNumber, oldValue, NotifyCollectionChangedAction.Replace);
            }
        }

        /// <summary>
        /// Sound file location for example.
        /// </summary>
        public object Tag { get; set; } = null;

        public string DefaultTimePointName => $"Time point {Id}";

        #endregion

        #region Methods

        // GetAbsolute
        /// <summary>
        /// Returns absolute time by baseTime
        /// </summary>
        /// <param name="baseTime"></param>
        /// <returns></returns>
        public TimeSpan GetAbsoluteTime(TimeSpan baseTime)
        {
            BaseTime = baseTime;

            if (TimePointType == TimePointType.Absolute)
                return Time;
            else
                return _GetAbsoluteTime();
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

            return _GetAbsoluteTime();
        }
        private TimeSpan _GetAbsoluteTime ()
        {
            if (BaseTime == TimeSpan.Zero) 
                return Time;

            // ReSharper disable once PossibleInvalidOperationException
            var res = (TimeSpan)BaseTime + Time;

            if (res.Days > 0)
                res -= TimeSpan.FromDays (1);

            return res;
        }

        // GetRelative
        public TimeSpan GetRelativeTime(TimeSpan baseTime)
        {
            if (TimePointType == TimePointType.Relative) {       
                return Time;
            }

            var oldBase = BaseTime;
            BaseTime = baseTime;

            var res = _GetRelativeTime();

            BaseTime = oldBase;
            return res;
        }
        public TimeSpan GetRelativeTime()
        {
            if (TimePointType == TimePointType.Relative)
                return Time;

            if (!BaseTime.HasValue) {
                throw new ArgumentException("BaseTime must be set");
            }

            return _GetRelativeTime();
        }
        private TimeSpan _GetRelativeTime ()
        {
            if (BaseTime == TimeSpan.Zero) 
                return Time;

            if (BaseTime == Time)
                return TimeSpan.Zero;

            TimeSpan res;

            if (Time <= BaseTime)
                res = TimeSpan.FromHours (24) - (TimeSpan)BaseTime + Time;
            else
                // ReSharper disable once PossibleInvalidOperationException
                res = Time - (TimeSpan)BaseTime;

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

        public TimePoint Copy()
        {
            var tp = TimePoint.DefaultTimePoint;
            tp.Time = this.Time;
            tp.TimePointType = this.TimePointType;
            tp.Tag = this.Tag;
            tp.BaseTime = this.BaseTime;

            return tp;
        }

        public override string ToString() => $"{Name}: {Time:h\\:mm\\:ss} ({TimePointType})"; 

        #endregion

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged (byte newValue, byte oldValue, NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (action, new List<byte> { newValue }, new List<byte> { oldValue }) );
        }
    }
}
