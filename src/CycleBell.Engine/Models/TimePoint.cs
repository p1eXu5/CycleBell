/*
 * Copyright © 2018 Vladimir Likhatskiy. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *  
 */

using System;
using System.Xml.Serialization;
using CycleBell.Engine.Exceptions;
using CycleBell.Engine.Models.Extensions;

namespace CycleBell.Engine.Models
{
    [Serializable]
    public class TimePoint : IEquatable<TimePoint>
    {
        #region static

        private static byte _timePointNum = Byte.MinValue + 1;

        static TimePoint()
        {
            GetDefaultName = (tp) => $"TimePoint {tp.Id}";
        }

        /// <summary>
        /// Дефолтное смещение временной точки относительно текущего времени
        /// </summary>
        public static TimeSpan DefaultTime { get; set; }

        public static TimePointKinds DefaultKind { get; set; } = TimePointKinds.Relative;

        /// <summary>
        /// Имя при null (для определения StartTime)
        /// </summary>-
        public static string FirstPointName { get; set; } = "Launch Time";

        public static int MaxId { get; } = UInt16.MaxValue;

        public static TimePoint GetAbsoluteTimePoint() => new TimePoint(time: TimeSpan.Zero, timePointKinds: TimePointKinds.Absolute);

        public static Func<TimePoint, String> GetDefaultName { get; set; }


        #endregion


        #region fields

        private string _name;
        private TimeSpan _time;
        private TimeSpan? _baseTime;
        private UInt16 _loopNumber;

        private TimePointKinds _kind;

        #endregion


        #region ctor

        public TimePoint()
            : this(
                name: null, 
                time: DefaultTime, 
                kind: DefaultKind)
        { }
        
        /// <summary>
        /// Creates TimePoint instance
        /// </summary>
        /// <param name="name">Name of TimePoint</param>
        /// <param name="time">An absolute or relative time of day</param>
        /// <param name="kind">A type of TimePoint</param>
        /// <param name="loopNumber">The loop number</param>
        public TimePoint(string name, TimeSpan time, TimePointKinds kind, byte loopNumber = 0)
        {
            if (_timePointNum == MaxId)
                throw new OverflowException ("Reached the maximum number of TimePointCollection");

            Time = time;

            if (kind == TimePointKinds.Absolute) {
                BaseTime = TimeSpan.Zero;
            }

            Id = _timePointNum++;

            _kind = kind;

            LoopNumber = loopNumber == MaxId ? (byte)(MaxId - 1) : loopNumber;

            Name = name;
        }

        public TimePoint(TimeSpan time, TimePointKinds timePointKinds, byte loopNumber = 0)
            : this(null, time, timePointKinds, loopNumber)
        { }

        public TimePoint(TimeSpan time)
            : this(time, DefaultKind)
        { }

        public TimePoint(string time)
            : this(TimeSpan.Parse( time ), DefaultKind)
        { }

        public TimePoint(string time, TimePointKinds kind, byte loopNumber = 0 )
            : this(TimeSpan.Parse( time ), kind, loopNumber)
        { }

        public TimePoint(string name, string time)
            : this(name, TimeSpan.Parse( time ), DefaultKind)
        { }

        public TimePoint(string name, string time, TimePointKinds kind, byte loopNumber = 0)
            : this(name, TimeSpan.Parse( time ), kind, loopNumber)
        { }

        #endregion


        #region events


        public event EventHandler< LoopNumberChangedEventArgs > LoopNumberChanged;

        protected virtual void OnLoopNumberChanged (int newValue, int oldValue )
        {
            LoopNumberChanged?.Invoke (this, new LoopNumberChangedEventArgs (newValue , oldValue ) );
        }

        #endregion


        #region properties

        /// <summary>
        /// Уникальный порядковый номер временной точки
        /// </summary>
        [XmlIgnore]
        public byte Id { get; }

        /// <summary>
        /// Name of NextTimePoint
        /// </summary>
        public string Name
        {
            get => _name ?? GetDefaultName(this);
            set => _name = value;
        }

        /// <summary>
        /// Absolute or relative time
        /// </summary>
        public TimeSpan Time
        {
            get => _time;
            set => _time = new TimeSpan(value.Hours, value.Minutes, value.Seconds);
        }

        /// <summary>
        /// Can be setted by <see cref="Preset.AddTimePoint"/> and <see cref="Timer.TimerQueueCalculator.GetTimerQueue"/>
        /// </summary>
        public TimeSpan? BaseTime
        {
            get => _baseTime;
            set {
                if (value == null) {
                    _baseTime = null;
                }
                else {
                    _baseTime = new TimeSpan(((TimeSpan)value).Hours, ((TimeSpan)value).Minutes, ((TimeSpan)value).Seconds);
                }
            }
        }

        /// <summary>
        /// Type of Time, absolute or relative
        /// </summary>
        public TimePointKinds Kind => _kind;

        /// <summary>
        /// Number of queue where this NextTimePoint will measure off
        /// </summary>
        public int LoopNumber 
        { 
            get => _loopNumber;
            set {
                var oldValue = _loopNumber;
                _loopNumber = Convert.ToUInt16(value);
                OnLoopNumberChanged( _loopNumber, oldValue );
            } 
        }

        /// <summary>
        /// Sound file location for example.
        /// </summary>
        public object Tag { get; set; }

        #endregion


        #region methods

        public void ChangeTimePointType(TimePointKinds newTimePointKinds)
        {
            switch (newTimePointKinds) {

                case TimePointKinds.Absolute:

                    if (Kind == TimePointKinds.Relative) {

                        if (BaseTime == null) {
                            Time = GetAbsoluteTime(TimeSpan.Zero);
                        }
                        else {
                            Time = GetAbsoluteTime();
                        }

                        _kind = TimePointKinds.Absolute;
                    }

                    break;

                case TimePointKinds.Relative:

                    if (Kind == TimePointKinds.Absolute) {

                        if (BaseTime == null) {
                            Time = GetRelativeTime(TimeSpan.Zero);
                        }
                        else {
                            Time = GetRelativeTime();
                        }
                        _kind = TimePointKinds.Relative;
                    }

                    break;
            }
        }

        /// <summary>
        /// Gets absolute time.
        /// </summary>
        /// <returns>Absolute time</returns>
        /// <exception cref="ArgumentException">Throws if BaseTime is null.</exception>
        public TimeSpan GetAbsoluteTime()
        {
            if (Kind == TimePointKinds.Absolute)
                return Time;

            if (!BaseTime.HasValue) {
                throw new BaseTimeNotSettedException(this);
            }

            if (BaseTime == TimeSpan.Zero) 
                return Time;

            // ReSharper disable once PossibleInvalidOperationException
            var res = (TimeSpan)BaseTime + Time;

            if (res.Days > 0)
                res -= TimeSpan.FromDays (res.Days);

            return res;
        }

        /// <summary>
        /// Returns absolute time by baseTime and sets BaseTime equaled to baseTime.
        /// </summary>
        /// <param name="baseTime"></param>
        /// <param name="preserveBaseTime">Does TimePoint BaseTime assigned new value?</param>
        /// <returns><see cref="TimeSpan"/></returns>
        public TimeSpan GetAbsoluteTime(TimeSpan baseTime, bool preserveBaseTime = true)
        {
            var oldBaseTime = BaseTime; 
            BaseTime = baseTime;

            var res = GetAbsoluteTime();

            if (!preserveBaseTime) {
                BaseTime = oldBaseTime;
            }

            return res;
        }

        // GetRelativeTime:

        /// <summary>
        /// Gets relative time.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws if BaseTime is null.</exception>
        public TimeSpan GetRelativeTime()
        {
            if (Kind == TimePointKinds.Relative)
                return Time;

            if (!BaseTime.HasValue) {
                throw new BaseTimeNotSettedException(this);
            }

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
        /// Returns relative time by baseTime and sets BaseTime equaled to baseTime.
        /// </summary>
        /// <param name="baseTime"></param>
        /// <returns><see cref="TimeSpan"/></returns>
        public TimeSpan GetRelativeTime(TimeSpan baseTime)
        {
            BaseTime = baseTime;

            return GetRelativeTime();
        }

        // Clone:
        public TimePoint Clone()
        {
            var tp = GetAbsoluteTimePoint();

            tp.Time = this.Time;

            tp.ChangeTimePointType(this._kind);
            tp.BaseTime = this.BaseTime;

            tp.Name = this.Name;
            tp.LoopNumber = this.LoopNumber;

            if (Tag != null) {
                tp.Tag = Tag.DeepCopy<object>();
            }

            return tp;
        }

        #endregion


        #region IEquatable
        
        public bool Equals (TimePoint other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        #endregion


        #region overrides

        public override string ToString() => $"{Name}: {Time:h\\:mm\\:ss}; ({Kind}); (l#: {LoopNumber}); (BaseTime: {(BaseTime == null ? "null" : BaseTime.ToString())})\n";

        public override bool Equals(object obj)
        {
            if (obj is TimePoint timePoint)
                return Equals (timePoint);
            
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}
