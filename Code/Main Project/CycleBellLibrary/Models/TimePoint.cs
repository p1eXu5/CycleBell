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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Remoting.Messaging;
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

        private static byte _timePointNum = Byte.MinValue + 1;

        private string _name;
        private TimeSpan _time;
        private TimeSpan? _baseTime;
        private byte _loopNumber;

        private static readonly Func<byte, String> _defaultTimePointNameFunc;
        private TimePointType _timePointType;

        #endregion

        #region Constructors

        // Static!
        static TimePoint()
        {
            DefaultTime = TimeSpan.Zero;
            TimePointNameFunc = _defaultTimePointNameFunc = (id) => $"TimePoint {id}";
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

            Time = time;

            if (timePointType == TimePointType.Absolute) {
                BaseTime = TimeSpan.Zero;
            }

            Id = _timePointNum++;

            _timePointType = timePointType;

            LoopNumber = loopNumber == MaxId ? (byte)(MaxId - 1) : loopNumber;

            Name = name ?? _defaultTimePointNameFunc(Id);
        }

        #region Linked

        public TimePoint(TimeSpan time, TimePointType timePointType)
            : this(null, time, timePointType)
        { }
        public TimePoint(string time, TimePointType timePointType, byte loopNumber = 0)
            : this(null, TimeSpan.Parse(time), timePointType, loopNumber)
        { }
        public TimePoint(TimeSpan time)
            :this(null, time, DefaultTimePointType)
        { }

        public TimePoint(string name)
            :this(name, DefaultTime, DefaultTimePointType)
        { }

        public TimePoint(string name, string time)
            :this(name, TimeSpan.Parse(time), DefaultTimePointType)
        { }
        public TimePoint(string name, string time, TimePointType timePointType, byte loopNumber = 0)
            :this(name, TimeSpan.Parse(time), timePointType, loopNumber)
        { }

        public TimePoint (string name, string time, TimeSpan baseTime, TimePointType timePointType, byte loopNumber = 0)
            : this (name, TimeSpan.Parse (time), timePointType, loopNumber)
        {
            BaseTime = baseTime;
        }

        #endregion

        #endregion

        #region Preperties Static 

        /// <summary>
        /// Дефолтное смещение временной точки относительно текущего времени
        /// </summary>
        public static TimeSpan DefaultTime { get; set; }

        public static TimePointType DefaultTimePointType { get; set; } = TimePointType.Relative;

        /// <summary>
        /// Имя при null (для определения StartTime)
        /// </summary>-
        public static string FirstPointName { get; set; } = "Launch Time";

        public static Byte MaxId { get; } = Byte.MaxValue;
        public static Byte MinId { get; } = Byte.MinValue;

        public static TimePoint GetAbsoluteTimePoint() => new TimePoint(time: TimeSpan.Zero, timePointType: TimePointType.Absolute);

        public static Func<byte, String> TimePointNameFunc { get; set; }

        #endregion

        #region Properties

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
            get => _name ?? FirstPointName;
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

        public static string GetDefaultTimePointName(TimePoint tp) { return TimePointNameFunc?.Invoke(tp.Id) ?? _defaultTimePointNameFunc.Invoke(tp.Id); }

        // GetAbsoluteTime:

        /// <summary>
        /// Gets absolute time.
        /// </summary>
        /// <returns>Absolute time</returns>
        /// <exception cref="ArgumentException">Throws if BaseTime is null.</exception>
        public TimeSpan GetAbsoluteTime()
        {
            if (TimePointType == TimePointType.Absolute)
                return Time;

            if (!BaseTime.HasValue) {
                throw new ArgumentException("BaseTime must be set");
            }

            return _GetAbsoluteTime();
        }

        /// <summary>
        /// Returns absolute time by baseTime and sets BaseTime equaled to baseTime.
        /// </summary>
        /// <param name="baseTime"></param>
        /// <returns><see cref="TimeSpan"/></returns>
        public TimeSpan GetAbsoluteTime(TimeSpan baseTime)
        {
            BaseTime = baseTime;

            if (TimePointType == TimePointType.Absolute)
                return Time;

            return _GetAbsoluteTime();
        }

        private TimeSpan _GetAbsoluteTime ()
        {
            if (BaseTime == TimeSpan.Zero) 
                return Time;

            // ReSharper disable once PossibleInvalidOperationException
            var res = (TimeSpan)BaseTime + Time;

            if (res.Days > 0)
                res -= TimeSpan.FromDays (res.Days);

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
            if (TimePointType == TimePointType.Relative)
                return Time;

            if (!BaseTime.HasValue) {
                throw new ArgumentException("BaseTime must be set");
            }

            return _GetRelativeTime();
        }

        /// <summary>
        /// Returns relative time by baseTime and sets BaseTime equaled to baseTime.
        /// </summary>
        /// <param name="baseTime"></param>
        /// <returns><see cref="TimeSpan"/></returns>
        public TimeSpan GetRelativeTime(TimeSpan baseTime)
        {
            BaseTime = baseTime;

            if (TimePointType == TimePointType.Relative) {       
                return Time;
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

        // Clone:
        public TimePoint Clone()
        {
            var tp = GetAbsoluteTimePoint();

            tp.Time = this.Time;

            tp.ChangeTimePointType(this._timePointType);
            tp.BaseTime = this.BaseTime;

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

        public override bool Equals(object obj)
        {
            if (obj is TimePoint timePoint)
                return Equals (timePoint);
            
            return false;
        }

        public bool Equals (TimePoint other)
        {
            if (other == null)
                return false;

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
