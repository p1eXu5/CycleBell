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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CycleBell.Engine.Models
{
    /// <summary>
    /// It's a TimePoint repository. If StartTime not set, getter returns negative TimeSpan (TimeSpan.FromSecond(-1))
    /// </summary>
    public class Preset
    {
        #region static

        static Preset()
        {
            DefaultStartTime = InitDefaultStartTime;
        }
        
        public static TimeSpan InitDefaultStartTime { get; } = TimeSpan.Zero;

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
        public static Preset GetDefaultPreset() => new Preset();

        public static bool IsDefaultPreset(Preset preset)
        {
            return (preset.PresetName.Equals(DefaultName))
                   && (!preset.TimePointCollection.Any())
                   && (preset.StartTime.Equals(DefaultStartTime));
        }

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

        public static void UpdateTimePointBaseTimes(IEnumerable<TimePoint> timePointCollection, TimeSpan startTime)
        {
            var array = timePointCollection.ToArray();

            array[0].BaseTime = startTime;

            for (int i = 1; i < array.Length; ++i) {

                array[i].BaseTime = array[i - 1].GetAbsoluteTime();
            }
        }
        
        #endregion


        #region Fields

        private TimeSpan _startTime;
        private ObservableCollection<TimePoint> _timePoints;
        private ReadOnlyObservableCollection<TimePoint> _readOnlyTimePointCollectionCollection;
        private byte _isInfiniteLoop;

        #endregion


        #region Constructors


        // Instance
        private Preset(string name, TimeSpan startTime)
        {
            PresetName = name ?? DefaultName;

            TimerLoopDictionary = new TimerLoopSerializableSortedDictionary();
            _timePoints = new ObservableCollection<TimePoint>();

            _readOnlyTimePointCollectionCollection = new ReadOnlyObservableCollection<TimePoint> (_timePoints);

            _startTime = startTime < TimeSpan.Zero ? startTime.Negate() : startTime;

            if (_startTime >= TimeSpan.FromDays(1)) {
                _startTime -= TimeSpan.FromDays(1);
            }
        }

        #region Linked Constructors

        public Preset()
           : this(DefaultName, DefaultStartTime)
        { }

        public Preset (string name)
            : this (name, DefaultStartTime)
        { }

        public Preset(TimeSpan startTime)
            : this(DefaultName, startTime)
        { }

        public Preset (IEnumerable<TimePoint> timePoints)
            : this (DefaultName, DefaultStartTime)
        {
            if (timePoints == null)
                throw new ArgumentNullException();

            var autoUpdt = AutoUpdateTimePointBaseTimes;

            if (autoUpdt)
                AutoUpdateTimePointBaseTimes = false;

            foreach (var timePoint in timePoints) {
                AddTimePoint (timePoint);
            }

            if (autoUpdt) {
                AutoUpdateTimePointBaseTimes = true;
                UpdateTimePointBaseTimes();
            }
        }

        #endregion

        #endregion


        #region Properties

        /// <summary>
        /// Start time for preset
        /// </summary>
        public TimeSpan StartTime
        {
            get => _startTime;
            set {
                TimeSpan startTime = value;

                if (startTime < TimeSpan.Zero) {
                    startTime = startTime.Negate();
                }

                if (startTime >= TimeSpan.FromDays(1)) {
                    startTime -= TimeSpan.FromDays(1);
                }

                _startTime = AutoUpdateTimePointBaseTimes ? OnStartTimeChanged(startTime, _startTime) : startTime;
            }
        }

        /// <summary>
        /// Indicates whether the loop is infinite
        /// </summary>
        public bool IsInfiniteLoop => _isInfiniteLoop != 0;


        public ReadOnlyObservableCollection<TimePoint> TimePointCollection => _readOnlyTimePointCollectionCollection;

        /// <summary>
        /// # cycle - n times
        /// </summary>
        public TimerLoopSerializableSortedDictionary TimerLoopDictionary { get; set; }

        public Object Tag { get; set; }

        #endregion


        #region Methods
        public virtual void PreRemoveTimePoint (TimePoint timePoint) { }

        public virtual IEnumerable<TimePoint> GetOrderedTimePoints() => _timePoints.OrderBy (tp => tp.LoopNumber).ThenBy (tp => tp.Id);

        /// <summary>
        /// If adding timePoint has Relative Kind and zero Time
        /// or adding timePoint less than zero then throw.
        /// </summary>
        /// <param name="timePoint">Adding TimePoint</param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void CheckTime( TimePoint timePoint )
        {
            if ((timePoint.Kind == TimePointKinds.Relative && timePoint.Time == TimeSpan.Zero)
                || (timePoint.Time < TimeSpan.Zero)) {
                throw new ArgumentException("Relative TimePoint can't have zero Time or be less than zero", nameof(timePoint));
            }
        }


        /// <summary>
        /// Add NextTimePoint.
        /// If added TimePoint Id is less than max Id of contained TimePoints, new TimePoint will be created and returned.
        /// </summary>
        /// <param name="timePoint"></param>
        /// <returns>Added TimePoint</returns>
        /// <exception cref="ArgumentNullException">when timePoint is null</exception>
        public TimePoint AddTimePoint(TimePoint timePoint)
        {
            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");
         
            if (TimePointCollection.Contains (timePoint))
                throw new ArgumentException("timePoint already exists", nameof(timePoint));

            CheckTime( timePoint );

            if ( TimePointCollection.Count > 0 ) 
            {
                var maxId = TimePointCollection.Max( tp => tp.Id );

                // If time point was created earlier then last time point in inner collection
                if (timePoint.Id <= maxId) {
                    timePoint = timePoint.Clone();
                }   
            }

            if ( AutoUpdateTimePointBaseTimes ) 
            {
                var points = new List< TimePoint >( TimePointCollection ) { timePoint };
                UpdateTimePointBaseTimes(points.OrderBy (tp => tp.LoopNumber).ThenBy (tp => tp.Id), StartTime);
            }

            _timePoints.Add( timePoint );
            // -> time point added to TimePointViewModelCollection 

            AddLoopNumberFrom( timePoint );
            timePoint.LoopNumberChanged += OnTimePointLoopNumberChanged;

            return timePoint;
        }

        public void AddTimePoints(IEnumerable<TimePoint> timePoints)
        {
            if (timePoints == null) {
                throw new ArgumentNullException(nameof(timePoints), "timePoints can't be null");
            }

            var tmp = timePoints.ToArray();

            if (!tmp.Any()) {
                return;
            }

            var autoUpdt = AutoUpdateTimePointBaseTimes;
            if (autoUpdt) {
                AutoUpdateTimePointBaseTimes = false;
            }

            foreach (var timePoint in tmp) {
                AddTimePoint(timePoint);
            }

            if (autoUpdt) {
                AutoUpdateTimePointBaseTimes = true;
                UpdateTimePointBaseTimes();
            }
        }

        public void RemoveTimePoint(TimePoint timePoint)
        {
            if (timePoint == null)
                throw new ArgumentNullException(nameof(timePoint), "timePoint can't be null");

            if (!_timePoints.Contains (timePoint))
                throw new ArgumentException("timePoint not in presetCollection", nameof(timePoint));

            PreRemoveTimePoint (timePoint);

            _timePoints.Remove(timePoint);

            if (TimePointCollection.FirstOrDefault (t => t.LoopNumber == timePoint.LoopNumber) == null) {

                TimerLoopDictionary.Remove(timePoint.LoopNumber);
            }

            if (TimePointCollection.Count > 0 && AutoUpdateTimePointBaseTimes)
                UpdateTimePointBaseTimes();
        }

        public void SetInfiniteLoop() => _isInfiniteLoop |= 0x01;

        public void ResetInfiniteLoop() => _isInfiniteLoop ^= _isInfiniteLoop;

        public Preset GetDeepCopy()
        {
            var timePoints = _timePoints;
            _timePoints = new ObservableCollection<TimePoint>(_timePoints);
            _readOnlyTimePointCollectionCollection = new ReadOnlyObservableCollection<TimePoint>(_timePoints);

            var timerLoops = TimerLoopDictionary;
            TimerLoopDictionary = new TimerLoopSerializableSortedDictionary();
            foreach (var key in timerLoops.Keys) {
                TimerLoopDictionary[key] = timerLoops[key];
            }

            var tag = Tag;
            Tag = new object();

            var preset = (Preset)this.MemberwiseClone();

            Tag = tag;

            TimerLoopDictionary = timerLoops;

            _timePoints = timePoints;
            _readOnlyTimePointCollectionCollection = new ReadOnlyObservableCollection<TimePoint>(_timePoints);

            return preset;
        }

        /// <summary>
        /// Updates TimePoint BaseTimes
        /// </summary>
        public void UpdateTimePointBaseTimes()
        {
            var array = GetOrderedTimePoints().ToArray();

            array[0].BaseTime = StartTime;

            for (int i = 1; i < array.Length; ++i) {

                    array[i].BaseTime = array[i - 1].GetAbsoluteTime();
            }
        }



        /// <summary>
        /// If adding TimePoint Id less then max Id in Preset PresetCollection
        /// than adding time point will be clonned for change Id to global current max
        /// </summary>
        /// <param name="timePoint">Adding TimePoint</param>
        private void PrepareTimePointId (ref TimePoint timePoint)
        {
            
        }

        private void AddLoopNumberFrom (TimePoint timePoint)
        {
            if (!TimerLoopDictionary.ContainsKey (timePoint.LoopNumber)) {
                TimerLoopDictionary[timePoint.LoopNumber] = 1;
            }
        }

        /// <summary>
        /// Calls when TimePoint LoopNumber changed
        /// </summary>
        /// <param name="sender">TimePoint</param>
        /// <param name="args">The new and old LoopNumbers</param>
        private void OnTimePointLoopNumberChanged (object sender, LoopNumberChangedEventArgs args)
        {
            if (sender is TimePoint tp) {
                
                AddLoopNumberFrom (tp);

                if (TimePointCollection.FirstOrDefault (t => t.LoopNumber == (Byte) args.OldLoopNumber) == null) {

                    this.TimerLoopDictionary.Remove ((Byte) args.OldLoopNumber);
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
            if (TimePointCollection.Count == 0)
                goto exit;

            var diff = newStartTime - oldStartTime;

            foreach (var timePoint in TimePointCollection) {

                timePoint.BaseTime += diff;

                if (timePoint.Kind == TimePointKinds.Absolute) {
                    timePoint.Time += diff;
                }
            }

            exit:
            return newStartTime;
        }

        #endregion


        #region overrides
        public override string ToString()
        {
            return new StringBuilder().Append("PresetName: ").Append(PresetName).Append("\n")
                                      .Append("StartTime: ").Append(StartTime).Append("\n")
                                      .Append("TimePoint's Count: ").Append(TimePointCollection.Count)
                                      .ToString();
        }
        
        #endregion
    }
}
