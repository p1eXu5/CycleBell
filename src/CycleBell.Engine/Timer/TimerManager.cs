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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer
{
    /// <summary>
    /// Timer manager. Creates timer's queue, emit signals
    /// every Accuacy of second, when time point has changes
    /// and when timer has stopped.
    ///
    /// The signals contains "last TimePoint", "next TimePoint" and last time
    /// 
    /// First TimePointChange signal emits with "last TimePoint" with
    /// negative Time, next TimePoint - created TimePoint with name preset.StartTimePointName
    /// </summary>
    public class TimerManager : ITimerManager, IStartTimePointCreator
    {
        #region consts

        public const string START_TIMEPOINT_NAME = "Launch in T-minus";
        public const string INITIAL_TIMEPOINT_NAME = "Initial TimePoint";

        #endregion


        #region static

        /// <summary>
        /// Instance of class
        /// </summary>
        private static TimerManager _timerManager;

        /// <summary>
        /// Gets instance of manager and if IOpenSave.FileName is Exist loads presets or loads only one empty preset
        /// </summary>
        /// <returns></returns>
        public static TimerManager Instance 
            => _timerManager ?? (_timerManager = new TimerManager());
        
        public static int Accuracy { get; set; } = 300;
        
        /// <summary>
        /// TimePoint with negate absolute time.
        /// </summary>
        public static TimePoint InitialTimePoint { get; } = new TimePoint(INITIAL_TIMEPOINT_NAME, TimeSpan.FromMinutes(-1), TimePointKinds.Absolute);

        public static bool IsInitialTimePoint( TimePoint timePoint )
        {
            return timePoint.Time < TimeSpan.Zero;
        }

        public static TimePoint GetStartTimePoint( TimeSpan startTime )
        {
            return new TimePoint( START_TIMEPOINT_NAME, startTime, TimePointKinds.Absolute );
        }

        #endregion


        #region fields

        private Preset _preset;

        /// <summary>
        /// The main queue. The next (TimeSpan startTimeForNextStartPoint, TimePoint nextPoint) always on the top.
        /// </summary>
        private Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)> _queue;

        /// <summary>
        /// Previous queue element
        /// </summary>
        private (TimeSpan prevChangeTime, TimePoint prevTimePoint) _prevQueueElement;

        /// <summary>
        /// Internal timer
        /// </summary>
        private System.Threading.Timer _timer;

        private TimeSpan _deltaTime;
        private bool _isRunAsync;
        private bool _isPreserveBaseTime = true;

        private readonly ITimerQueueCalculator _timerQueueCalculator;

        private readonly object _locker = new object();
        private readonly object _locker2 = new object();
        private readonly object _locker3 = new object();
        private bool _isJustRunned;

        #endregion


        #region ctor

        private TimerManager()
        {
            _timerQueueCalculator = new TimerQueueCalculator (this);
        }

        private TimerManager(ITimerQueueCalculator timerQueueCalculator)
        {
            _timerQueueCalculator = timerQueueCalculator;
        }

        #endregion


        #region events

        public event EventHandler<TimerEventArgs> TimePointChanged;
        public event EventHandler<TimerEventArgs> SecondPassed;
        public event EventHandler TimerPaused;
        public event EventHandler TimerStopped;
        public event EventHandler TimerStarted;

        #endregion


        #region properties

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public string StartTimePointName => START_TIMEPOINT_NAME;

        #endregion


        #region methods

        public async void PlayAsync( Preset preset )
        {
            if (IsRunning)
                return;

            await Task.Run(() => Play(preset));
        }

        /// <summary>
        /// Запуск
        /// </summary>
        /// <param name="preset">Запускаемый пресет</param>
        public void Play( Preset preset )
        {
            _preset = preset;

            if (IsRunning && !_isRunAsync)
                return;

            _queue = _timerQueueCalculator.GetTimerQueue (preset, _isPreserveBaseTime);


            if (_queue == null) {

                Stop();
                return;
            }

            ResetDeltaTime();

            var currentTime = DateTime.Now.TimeOfDay;

            _prevQueueElement = ( currentTime, InitialTimePoint );

            OnTimerStart();
            _timer = new System.Threading.Timer(TimerCallbackHandler, null, 0, Timeout.Infinite);
        }

        /// <summary>
        /// Pause timer loop
        /// </summary>
        public void Pause()
        {
            if (!IsRunning) return; 
                
            _timer.Change (Timeout.Infinite, Timeout.Infinite);
            IsPaused = true;
            OnTimerPause();
        }

        /// <summary>
        /// Resume timer loop
        /// </summary>
        public void Resume()
        {
            if (!IsPaused) return;

            FindNextTimePoint();

            //OnChangeTimePoint(_prevQueueElement.prevTimePoint, foundedNextQueueElem, currentTime);

            ResetDeltaTime();
            _timer.Change( 0, Timeout.Infinite );

            IsPaused = false;
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            _timer?.Dispose();

            //_queue = null;
            if (IsRunning) IsRunning = false;
            if (_isRunAsync) _isRunAsync = false;
            if (IsPaused) IsPaused = false;

            OnTimerStop();
        }

        public void DontPreserveBaseTime() => _isPreserveBaseTime = false;

        public void PreserveBaseTime() => _isPreserveBaseTime = true;


        /// <summary>
        /// Invokes TimerStarted event.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimerStart()
        {
            TimerStarted?.Invoke(this, EventArgs.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimerPause()
        {
            TimerPaused?.Invoke(this, EventArgs.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimerStop()
        {
            TimerStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// dueTime for _timer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetDueTime (int i) => Accuracy - (i % Accuracy);

        /// <summary>
        /// Find next NextTimePoint after resume
        /// </summary>
        private void FindNextTimePoint()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            if (currentTime < _queue.Peek().nextChangeTime || _prevQueueElement.prevTimePoint?.Equals( InitialTimePoint ) == true ) {
                return;
            }

            do {
                _prevQueueElement = _queue.Dequeue();
                _queue.Enqueue(_prevQueueElement);
            } while (currentTime >= _queue.Peek().nextChangeTime && _queue.Peek().nextTimePoint != null);

        }

        // timer callback:
        /// <summary>
        /// Timer handler
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallbackHandler( object state )
        {
            var currentTime = DateTime.Now.TimeOfDay;

            var (nextTime, nextPoint) = _queue.Peek();
            var dt = CalculateLastTime( currentTime, nextTime );

            OnTimerSecondPassed( nextPoint, dt );

            if ( dt > _deltaTime || _deltaTime == TimeSpan.Zero ) {
                if ( !ChangeTimePoint() ) {
                    Stop();
                    return;
                }

                OnChangeTimePoint( _prevQueueElement.prevTimePoint, _queue.Peek(), dt );
                lock ( _locker2 ) {
                    _deltaTime = TimeSpan.FromHours( 25 );
                }
            }
            else {
                lock ( _locker ) {
                    _deltaTime = dt;
                }
            }

            currentTime = DateTime.Now.TimeOfDay;
            _timer.Change( GetDueTime( currentTime.Milliseconds ), Timeout.Infinite );
        }

        /// <summary>
        /// Change NextTimePoint in timer queue
        /// </summary>
        private bool ChangeTimePoint()
        {
            lock ( _locker3 ) {
                if ( _isJustRunned ) {
                    _isJustRunned = false;
                    return true;
                }

                _prevQueueElement = _queue.Dequeue();
                _queue.Enqueue( _prevQueueElement );
            }

            // If StartTimePoint are the next TimePoint:
            if ( _queue.Peek().nextTimePoint.Name == START_TIMEPOINT_NAME) {

                if ( !_preset.IsInfiniteLoop ) {
                    return false;
                }

            }
            
            return true;
        }

        /// <summary>
        /// _deltaTime = TimeSpan.FromHours( 25 ).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetDeltaTime() 
        {
             _deltaTime = TimeSpan.Zero;
            _isJustRunned = IsRunning = true;
        }

        // events:
        private void OnChangeTimePoint ( TimePoint prevTimePoint, (TimeSpan nextChangeTime, TimePoint nextTimePoint) nextQueueElement, TimeSpan lastTime )
        {
            // if previous TimePoint is the initial out of queue TimePoint or the StartPoint then
            // modify previous base time TimePoint will unnecessary
            if ( prevTimePoint.Time < TimeSpan.Zero || prevTimePoint.Name == StartTimePointName ) {

                TimePointChanged?.Invoke(this, new TimerEventArgs( prevTimePoint, nextQueueElement.nextTimePoint, lastTime, null ));
            }
            else {

                TimeSpan nextPrevTimePointAbsoluteTime = prevTimePoint.GetAbsoluteTime();
                TimeSpan? nextBaseTime = null;

                foreach (var queueElement in _queue.ToArray()) {

                    if (queueElement.nextTimePoint.Equals(prevTimePoint) && queueElement.nextChangeTime != nextPrevTimePointAbsoluteTime) {

                        var relativeTime = prevTimePoint.GetRelativeTime();
                        nextBaseTime = queueElement.nextChangeTime >= relativeTime
                                               ? queueElement.nextChangeTime - relativeTime
                                               : TimeSpan.FromDays(1) + queueElement.nextChangeTime - relativeTime;

                        break;
                    }
                }

                TimePointChanged?.Invoke(this, new TimerEventArgs(prevTimePoint, nextQueueElement.nextTimePoint, lastTime, nextBaseTime));
            }

        }

        private void OnTimerSecondPassed(TimePoint nextTimePoint, TimeSpan lastTime)
        {
            SecondPassed?.Invoke(this, new TimerEventArgs(null, nextTimePoint, lastTime, null));
        }

        /// <summary>
        /// Calculate time to the next TimePoint change
        /// </summary>
        /// <param name="currentTime">current time reference</param>
        /// <param name="nextTime"></param>
        /// <returns>Last time to nextTime</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TimeSpan CalculateLastTime(TimeSpan currentTime, TimeSpan nextTime)
        {
            TimeSpan diff;

            if (currentTime > nextTime) {
                diff = TimeSpan.FromHours( 24 ) - new TimeSpan( currentTime.Hours, currentTime.Minutes, currentTime.Seconds ) + nextTime;
            }
            else {
                diff = nextTime - new TimeSpan( currentTime.Hours, currentTime.Minutes, currentTime.Seconds );
            }

            return diff;
        }

        TimePoint IStartTimePointCreator.GetStartTimePoint(TimeSpan startTime)
        {
            return GetStartTimePoint(startTime);
        }

        #endregion Methods
    }

    #region Extension Class

    /// <summary>
    /// Extension class
    /// </summary>
    public static class PresetExtention
    {
        public static void RunTimer (this Preset preset, TimerManager manager)
        {
            manager.PlayAsync (preset);
        }
    }

    #endregion
}
