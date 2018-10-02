/*
 *
 *  Copyright (c) p1eXu5. All rights reserved.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Timer
{
    /// <summary>
    /// Timer manager. Creates timer's queue, emit signals
    /// every Accuacy of second, when time point has changes
    /// and when timer has stopped.
    ///
    /// The signals contains last TimePoint, next TimePoint and last time
    /// 
    /// First TimePointChange signal emits with last TimePoint with
    /// negative Time, next TimePoint - created TimePoint with name preset.StartTimePointName
    /// </summary>
    public class TimerManager : ITimerManager, IStartTimeTimePointName
    {
        public const string RestartTimePointString = "Restart";
        public const string StartTimeTimePointString = "Launch in T-minus";

        #region Fields

        /// <summary>
        /// Instance of class
        /// </summary>
        private static TimerManager _timerManager;

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
        private byte _isInfiniteLoop;
        private bool _isRunAsync;

        private readonly IBaseTimeCalculator _baseTimeCalculator;

        #endregion

        #region Constructor

        private TimerManager()
        {
            _baseTimeCalculator = new BaseTimeCalculator (this);
        }

        private TimerManager(IBaseTimeCalculator baseTimeCalculator)
        {
            _baseTimeCalculator = baseTimeCalculator;
        }

        /// <summary>
        /// Gets instance of manager and if IPresetCollectionManager.FileName is Exist loads presets or loads only one empty preset
        /// </summary>
        /// <returns></returns>
        public static TimerManager Instance 
            => _timerManager ?? (_timerManager = new TimerManager());
        
        public static TimerManager GetInstance (IBaseTimeCalculator baseTimeCalculator)
            => _timerManager ?? (_timerManager = new TimerManager(baseTimeCalculator));

        #endregion

        #region Events

        public event EventHandler<TimerEventArgs> ChangeTimePointEvent;

        private void OnChangeTimePoint(TimePoint prevTimePoint, TimePoint nextTimePoint, TimeSpan lastTime)
        {
            ChangeTimePointEvent?.Invoke(this, new TimerEventArgs(prevTimePoint, nextTimePoint, lastTime));
        }

        public event EventHandler<TimerEventArgs> TimerSecondPassedEvent;

        private void OnTimerSecondPassed(TimePoint nextTimePoint, TimeSpan lastTime)
        {
            TimerSecondPassedEvent?.Invoke(this, new TimerEventArgs(null, nextTimePoint, lastTime));
        }

        public event EventHandler TimerStopEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimerStop()
        {
            TimerStopEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TimerStartEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimerStart()
        {
            TimerStartEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public static int Accuracy { get; set; } =300;

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public string StartTimeTimePointName => TimerManager.StartTimeTimePointString;

        #endregion

        #region Methods

        /// <summary>
        /// dueTime for _timer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetDueTime (int i) => Accuracy - (i % Accuracy);

        /// <summary>
        /// Pause timer loop
        /// </summary>
        public void Pause()
        {
            if (!IsRunning) return; 
                
            _timer.Change (Timeout.Infinite, Timeout.Infinite);
            IsPaused = true;
        }

        /// <summary>
        /// Resume timer loop
        /// </summary>
        public void Resume()
        {
            if (!IsPaused) return;

            var currentTime = DateTime.Now.TimeOfDay;
            var foundedNextQueueElem = FindNextTimePoint(ref currentTime);

            OnChangeTimePoint(_prevQueueElement.prevTimePoint, foundedNextQueueElem.nextTimePoint, LastTime(currentTime, foundedNextQueueElem.nextChangeTime));

            _timer.Change(GetDueTime (currentTime.Milliseconds), Timeout.Infinite);

            IsPaused = false;
        }

        /// <summary>
        /// Find next NextTimePoint after resume
        /// </summary>
        /// <param name="currentTime"></param>
        private (TimeSpan nextChangeTime, TimePoint nextTimePoint) FindNextTimePoint(ref TimeSpan currentTime)
        {
            if (currentTime < _queue.Peek().nextChangeTime || _prevQueueElement.prevTimePoint.Time < TimeSpan.Zero) {
                return _queue.Peek();
            }

            do {
                _prevQueueElement = _queue.Dequeue();
                _queue.Enqueue(_prevQueueElement);
            } while (currentTime >= _queue.Peek().nextChangeTime && _queue.Peek().nextTimePoint != null);

            return _queue.Peek();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            _timer?.Dispose();

            _queue = null;
            if (IsRunning) IsRunning = false;
            if (_isRunAsync) _isRunAsync = false;
            if (IsPaused) IsPaused = false;

            OnTimerStop();
        }

        public async void PlayAsync(Preset preset)
        {
            if (IsRunning)
                return;

            await Task.Run(() => Play(preset));
        }

        /// <summary>
        /// Запуск
        /// </summary>
        /// <param name="preset">Запускаемый пресет</param>
        public void Play(Preset preset)
        {
            if (IsRunning && !_isRunAsync)
                return;

            _queue = _baseTimeCalculator.GetTimerQueue (preset);


            if (_queue == null) {

                Stop();
                return;
            }

            IsRunning = true;
            OnTimerStart();

            var currentTime = DateTime.Now.TimeOfDay;

            if (preset.IsInfiniteLoop)
                _isInfiniteLoop = 1;
            else
                _isInfiniteLoop ^= _isInfiniteLoop;

            _deltaTime = -TimeSpan.FromHours(1);

            // Timer initialize
            var durTime = GetDueTime(currentTime.Milliseconds);
            _timer = new System.Threading.Timer(TimerCallbackHandler, null, durTime, Timeout.Infinite);

            // Set previous queue element
            _prevQueueElement = (currentTime, new TimePoint(StartTimeTimePointString, TimeSpan.FromMinutes(-1), TimePointType.Absolute));

            OnChangeTimePoint(_prevQueueElement.prevTimePoint, _queue.Peek().nextTimePoint, LastTime(currentTime, _queue.Peek().nextChangeTime));
        }

        /// <summary>
        /// Creates alarm queue
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <returns>The queue of tuples consists of time of the day and TimePoint that will come in this time</returns>
        //public Queue<(TimeSpan, TimePoint)> GetTimerQueue(Preset preset)
        //{
        //    if (preset?.TimePointCollection == null || preset.TimePointCollection.Count == 0)
        //        return null;

        //    // Смещение по времени следующей временной точки
        //    TimeSpan startTime = preset.StartTime;

        //    // Очередь кортежей времени будильника и соответствующей ему NextTimePoint
        //    Queue<(TimeSpan, TimePoint)> queue = new Queue<(TimeSpan, TimePoint)>();

        //    queue.Enqueue((startTime, new TimePoint(StartTimeTimePointName, startTime, TimePointType.Absolute)));

        //    // Заполняем очередь

        //    // Для всех временных сегментов
        //    foreach (var timerCycle in preset.TimerLoops.Keys) {

        //        TimeSpan nextTime;

        //        if (preset.TimePointCollection.Count > 1) {

        //            // Список временных точек каждого временного сегмента, порядоченный по Id (по порядку создания)
        //            var timePoints = preset.TimePointCollection.Where(t => t.LoopNumber == timerCycle).OrderBy(t => t.Id)
        //                                   .ToList();

        //            for (var i = 0; i < preset.TimerLoops[timerCycle]; ++i) {

        //                foreach (var point in timePoints) {

        //                    nextTime = point.GetAbsoluteTime(startTime);

        //                    queue.Enqueue((nextTime, point));
        //                    startTime = point.GetAbsoluteTime(startTime);
        //                }
        //            }
        //        }
        //        else {

        //            // If TimePointCollection.Count == 1
        //            var timePoint = preset.TimePointCollection[0];

        //            for (var i = 0; i < preset.TimerLoops[timerCycle]; ++i) {

        //                nextTime = timePoint.GetAbsoluteTime(startTime);

        //                queue.Enqueue((nextTime, timePoint));
        //                startTime = timePoint.GetAbsoluteTime(startTime);
        //            }
        //        }
        //    }

        //    return queue;
        //}

        /// <summary>
        /// Timer handler
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallbackHandler(object state)
        {
            var nextTime = _queue.Peek().Item1;
            var currentTime = DateTime.Now.TimeOfDay;

            _timer.Change(GetDueTime (currentTime.Milliseconds), Timeout.Infinite);

            if (currentTime > nextTime) {

                if (_deltaTime < TimeSpan.Zero) {

                    // Первый запуск или после смены NextTimePoint
                    _deltaTime = TimeSpan.FromHours(25);
                }
                else {

                    if (_deltaTime > TimeSpan.FromHours(24)) {

                        // Точка входа в ChangeTimePoint
                        ChangeTimePoint(ref currentTime);
                        return;
                    }
                }

                var deltaTime = nextTime + TimeSpan.FromHours(24) - currentTime;

                if (deltaTime > _deltaTime) {
                    ChangeTimePoint(ref currentTime);
                }
                else {

                    OnTimerSecondPassed(_queue.Peek().Item2, deltaTime);
                    _deltaTime = deltaTime;
                }

                return;
            }

            if (_deltaTime < TimeSpan.Zero) {

                // Первый запуск или после смены NextTimePoint
                _deltaTime = TimeSpan.FromHours(25);
            }

            OnTimerSecondPassed(_queue.Peek().Item2, (nextTime - currentTime));
        }

        /// <summary>
        /// Change NextTimePoint in timer queue
        /// </summary>
        /// <param name="currentTime">current time will be changed!</param>
        private void ChangeTimePoint(ref TimeSpan currentTime)
        {
            // Сообщаем, что прошла секунда, время истекло, следующую точку.
            OnTimerSecondPassed(_queue.Peek().Item2, TimeSpan.Zero);

            _prevQueueElement = _queue.Dequeue();
            _queue.Enqueue(_prevQueueElement);

            // If StartTimePoint are next:
            if (_queue.Peek().nextTimePoint.Name == TimerManager.StartTimeTimePointString) {

                // Если цикл не бесконечный:
                if (_isInfiniteLoop == 0) {

                    Stop();
                    return;
                }

                OnChangeTimePoint(_prevQueueElement.prevTimePoint, _queue.Peek().nextTimePoint, LastTime(currentTime, _queue.Peek().Item1));
                return;
            }


            OnChangeTimePoint(_prevQueueElement.prevTimePoint, _queue.Peek().nextTimePoint, LastTime(currentTime, _queue.Peek().nextChangeTime));

            // Если время следующей точки равно предыдущей:
            if (_queue.Peek().Item1 == _prevQueueElement.Item1) {

                ChangeTimePoint(ref currentTime);
                return;
            }

            _deltaTime = -TimeSpan.FromHours(1);
        }

        /// <summary>
        /// Calculate time to the next TimePoint change
        /// </summary>
        /// <param name="currentTime">current time reference</param>
        /// <param name="nextTime"></param>
        /// <returns>Last time to nextTime</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TimeSpan LastTime(TimeSpan currentTime, TimeSpan nextTime)
        {
            TimeSpan diff;

            if (currentTime > nextTime) {
                diff = nextTime + TimeSpan.FromHours(24) - currentTime;
            }
            else {
                diff = nextTime - currentTime;
            }

            return diff;
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
