using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace CycleBellLibrary.Models
{
    public enum TimePointType : byte
    {
        Relative = 0x01,
        Absolute = 0x02
    }

    [Serializable]
    public class TimePoint : INotifyCollectionChanged, IEquatable<TimePoint>
    {
        #region Fields

        public static readonly TimeSpan InitialDefaultTime = TimeSpan.Zero;

        private static byte _timePointNum = Byte.MinValue + 1;

        private string _name;
        private TimeSpan _time;
        private byte _loopNumber;

        private static Func<TimePoint, String> _defaultTimePointNameFunc;
        private TimePointType _timePointType;

        #endregion

        #region Constructors

        // Static!
        static TimePoint()
        {
            DefaultTime = InitialDefaultTime;
            //DefaultTimePointNameFunc = (tp) => $"TimePoint {tp.Id}";

            // Да хуй его, прикольно было попробовать
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
                name: null, 
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
                throw new OverflowException ("Reached the maximum number of TimePointCollection");

            Id = _timePointNum++;

            Time = time;
            _timePointType = timePointType;

            if (timePointType == TimePointType.Absolute)
                BaseTime = TimeSpan.Zero;

            LoopNumber = loopNumber == Byte.MaxValue ? (byte)(Byte.MaxValue - 1) : loopNumber;

            if (String.IsNullOrWhiteSpace (name)) {
                Name = GetDefaultTimePointName();
            }
            else {
                Name = name;
            }
        }

        #region Linked

        public TimePoint(TimeSpan time, TimePointType timePointType)
            : this("Time point " + _timePointNum, time, timePointType)
        { }
        public TimePoint(string time, TimePointType timePointType, byte loopNumber = 0)
            : this("Time point " + _timePointNum, TimeSpan.Parse(time), timePointType, loopNumber)
        { }
        public TimePoint(string time)
            :this("Time point " + _timePointNum, TimeSpan.Parse(time), DefaultTimePointType)
        { }
        public TimePoint(string name, string time)
            :this(name, TimeSpan.Parse(time), DefaultTimePointType)
        { }
        public TimePoint(string name, string time, TimePointType timePointType, byte loopNumber = 0)
            :this(name, TimeSpan.Parse(time), timePointType, loopNumber)
        { }

        #endregion

        #endregion

        #region Properties

            #region Static

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

        public static Func<TimePoint, String> DefaultTimePointNameFunc
        {
            get => _defaultTimePointNameFunc;
            set => _defaultTimePointNameFunc = value;
        }

            #endregion

        /// <summary>
        /// Уникальный порядковый номер временной точки
        /// </summary>
        [XmlIgnore]
        public int Id { get; }

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

        public TimeSpan? BaseTime { get; set; }

        /// <summary>
        /// Type of Time, absolute or relative
        /// </summary>
        public TimePointType TimePointType => _timePointType;

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
        public object Tag { get; set; }

        #endregion

        #region Methods

        public void ChangeTimePointType(TimePointType newTimePointType)
        {
            switch (newTimePointType) {

                case TimePointType.Absolute:

                    if (TimePointType == TimePointType.Relative) {

                        if (BaseTime == null) {
                            Time = GetAbsoluteTime(TimeSpan.Zero);
                        }
                        else {
                            Time = GetAbsoluteTime();
                        }

                        _timePointType = TimePointType.Absolute;
                    }

                    break;

                case TimePointType.Relative:

                    if (TimePointType == TimePointType.Absolute) {

                        if (BaseTime == null) {
                            Time = GetRelativeTime(TimeSpan.Zero);
                        }
                        else {
                            Time = GetRelativeTime();
                        }
                        _timePointType = TimePointType.Relative;
                    }

                    break;
            }
        }

        public string GetDefaultTimePointName() { return DefaultTimePointNameFunc?.Invoke(this) ?? ""; }

        // GetAbsolute
        /// <summary>
        /// Returns absolute time by baseTime
        /// </summary>
        /// <param name="baseTime"></param>
        /// <returns></returns>
        public TimeSpan GetAbsoluteTime(TimeSpan baseTime)
        {
            if (BaseTime == null)
                BaseTime = baseTime;

            if (TimePointType == TimePointType.Absolute)
                return Time;
            else
                return _GetAbsoluteTime(baseTime);
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

            return _GetAbsoluteTime((TimeSpan)BaseTime);
        }
        private TimeSpan _GetAbsoluteTime (TimeSpan baseTime)
        {
            if (baseTime == TimeSpan.Zero) 
                return Time;

            // ReSharper disable once PossibleInvalidOperationException
            var res = baseTime + Time;

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

        public TimePoint Clone()
        {
            var tp = DefaultTimePoint;

            tp.BaseTime = this.BaseTime;
            tp.Time = this.Time;
            tp.ChangeTimePointType(this._timePointType);

            tp.Name = this.Name;
            tp.LoopNumber = this.LoopNumber;

            if (Tag != null) {
                tp.Tag = Tag.CopyObject<object>();
            }

            return tp;
        }

        public override string ToString() => $"{Name}: {Time:h\\:mm\\:ss}\n({TimePointType})\n(l#: {LoopNumber})\n(BaseTime: {(BaseTime == null ? "null" : BaseTime.ToString())})\n";

        #endregion

        #region Operators

        public static bool operator == (TimePoint tp1, TimePoint tp2)
        {
            if (ReferenceEquals(tp1, null) || ReferenceEquals(tp2, null)) {

                return ReferenceEquals(tp1, tp2);
            }

            var res = (String.Equals(tp1.Name, tp2.Name))
                && (tp1.Time == tp2.Time)
                && (tp1.BaseTime == tp2.BaseTime)
                && (tp1.TimePointType == tp2.TimePointType)
                && (tp1.LoopNumber == tp2.LoopNumber)
                && (Object.Equals(tp1.Tag, tp2.Tag));

            return res;
        }

        public static bool operator != (TimePoint tp1, TimePoint tp2)
        {
            return !(tp1 == tp2);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals (null, obj)) return false;
            if (ReferenceEquals (this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals ((TimePoint) obj);
        }

        public bool Equals (TimePoint other)
        {
            if (ReferenceEquals (null, other)) return false;
            if (ReferenceEquals (this, other)) return true;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged (byte newValue, byte oldValue, NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke (this, new NotifyCollectionChangedEventArgs (action, new List<byte> { newValue }, new List<byte> { oldValue }) );
        }

        #endregion

    }
}
