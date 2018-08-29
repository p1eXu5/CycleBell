/*
 *
 *  Copyright (c) p1eXu5. All rights reserved.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CycleBellLibrary
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
    public class TimerManager : ITimerManager
    {
        public const string RestartString = "Restart";

        #region Fields

        /// <summary>
        /// Instance of class
        /// </summary>
        private static TimerManager _timerManager;

        /// <summary>
        /// Дирехтор
        /// </summary>
        private readonly IPresetsManager _presetsManager;

        /// <summary>
        /// The main queue. The next (TimeSpan startTimeForNextStartPoint, TimePoint nextPoint) always on the top.
        /// </summary>
        private Queue<(TimeSpan, TimePoint)> _queue;

        /// <summary>
        /// Internel timer
        /// </summary>
        private Timer _timer;

        private byte _isRunning;
        private TimeSpan _deltaTime;
        private byte _isInfiniteLoop;

        /// <summary>
        /// Previous queue element
        /// </summary>
        private (TimeSpan, TimePoint) _prevQueueElement;

        #endregion

        #region Constructor

        private TimerManager(IPresetsManager presetsManager)
        {
            // Устанавливаем дирехтора
            _presetsManager = presetsManager;
        }

        /// <summary>
        /// Gets instance of manager and if IPresetsManager.FileName is Exist loads presets or loads only one empty preset
        /// </summary>
        /// <param name="presetsManager"></param>
        /// <returns></returns>
        public static TimerManager Instance(IPresetsManager presetsManager) 
            => _timerManager ?? (_timerManager = new TimerManager(presetsManager));
        
        #endregion

        #region Events
        public event NotifyCollectionChangedEventHandler PresetCollectionChanged
        {
            add => _presetsManager.CollectionChanged += value;
            remove => _presetsManager.CollectionChanged -= value;
        }

        public event EventHandler<TimePointEventArgs> ChangeTimePointEvent;

        private void OnChangeTimePoint(TimePoint prevTimePoint, TimePoint nextTimePoint, TimeSpan lastTime)
        {
            ChangeTimePointEvent?.Invoke(this, new TimePointEventArgs(prevTimePoint, nextTimePoint, lastTime));
        }

        public event EventHandler<TimePointEventArgs> TimerSecondPassedEvent;

        private void OnTimerSecondPassed(TimePoint nextTimePoint, TimeSpan lastTime)
        {
            TimerSecondPassedEvent?.Invoke(this, new TimePointEventArgs(null, nextTimePoint, lastTime));
        }

        public event EventHandler TimerStopEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimerStop()
        {
            TimerStopEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public static int Accuracy { get; set; } =300;

        public ReadOnlyObservableCollection<Preset> Presets => _presetsManager.Presets;
        public bool IsRunning => _isRunning != 0;

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
        public void Pouse()
        {
            if (!IsRunning) return; 
                
            _timer.Change (Timeout.Infinite, Timeout.Infinite);
            _isRunning ^= _isRunning;
        }

        /// <summary>
        /// Resume timer loop
        /// </summary>
        public void Resume()
        {
            if (IsRunning || _queue == null) return;

            var currentTime = DateTime.Now.TimeOfDay;
            var foundedNextQueueElem = FindNextTimePoint(ref currentTime);

            OnChangeTimePoint(_prevQueueElement.Item2, foundedNextQueueElem.Item2, LastTime(currentTime, foundedNextQueueElem.Item1));

            _timer.Change(GetDueTime (currentTime.Milliseconds), Timeout.Infinite);
            _isRunning |= 0x01;
        }

        /// <summary>
        /// Find next NextTimePoint after resume
        /// </summary>
        /// <param name="currentTime"></param>
        private (TimeSpan, TimePoint) FindNextTimePoint(ref TimeSpan currentTime)
        {
            // Если текущее время меньше времени следующей точки или следующая точка - это startTime:
            if (currentTime < _queue.Peek().Item1 || _queue.Peek().Item2.Time < TimeSpan.Zero) {
                return _queue.Peek();
            }

            do {
                _prevQueueElement = _queue.Dequeue();
                _queue.Enqueue(_prevQueueElement);
            } while (currentTime >= _queue.Peek().Item1 && _queue.Peek().Item2 != null);

            return _queue.Peek();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            _timer?.Dispose();

            _queue = null;

            if (IsRunning)
                _isRunning ^= _isRunning;

            OnTimerStop();
        }

        public async void PlayAsync(Preset preset)
        {
            await Task.Run(() => Play(preset));
        }

        /// <summary>
        /// Запуск
        /// </summary>
        /// <param name="preset">Запускаемый пресет</param>
        public void Play(Preset preset)
        {
            if (IsRunning)
                return;

            _queue = GetTimerQueue(preset);

            if (_queue == null) {

                Stop();
                return;
            }

            _isRunning |= 0x01;

            var currentTime = DateTime.Now.TimeOfDay;

            // Определяем бесконечный ли цикл
            if (preset.IsInfiniteLoop)
                _isInfiniteLoop = 1;
            else
                _isInfiniteLoop ^= _isInfiniteLoop;

            _deltaTime = -TimeSpan.FromHours(1);

            // Инициируем таймер
            var durTime = GetDueTime(currentTime.Milliseconds);
            _timer = new Timer(TimerCallbackHandler, null, durTime, Timeout.Infinite);

            // Set previous queue element
            _prevQueueElement = (currentTime, new TimePoint("Launch Time", TimeSpan.FromMinutes(-1), TimePointType.Absolute));

            OnChangeTimePoint(_prevQueueElement.Item2, _queue.Peek().Item2, LastTime(currentTime, _queue.Peek().Item1));
        }

        /// <summary>
        /// Creates alarm queue
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <returns>The queue of tuples consists of time of the day and TimePoint that will come</returns>
        public static Queue<(TimeSpan, TimePoint)> GetTimerQueue(Preset preset)
        {
            if (preset?.TimePoints == null || preset.TimePoints.Count == 0)
                return null;

            // Очередь кортежей времени будильника и соответствующей ему NextTimePoint
            Queue<(TimeSpan, TimePoint)> queue = new Queue<(TimeSpan, TimePoint)>();

            // Смещение по времени следующей временной точки
            TimeSpan localStartTime = preset.StartTime;
            queue.Enqueue((localStartTime, new TimePoint(preset.StartTimePointName, localStartTime, TimePointType.Absolute)));

            // Заполняем очередь

            // Для всех временных сегментов
            foreach (var timerCycle in preset.TimerLoops.Keys) {

                TimeSpan nextTime;

                if (preset.TimePoints.Count > 1) {

                    // Список временных точек каждого временного сегмента, порядоченный по Id (по порядку создания)
                    var timePoints = preset.TimePoints.Where(t => t.LoopNumber == timerCycle).OrderBy(t => t.Id)
                                           .ToList();

                    for (var i = 0; i < preset.TimerLoops[timerCycle]; ++i) {

                        foreach (var point in timePoints) {

                            nextTime = point.GetAbsoluteTime(localStartTime);

                            queue.Enqueue((nextTime, point));
                            localStartTime = point.GetAbsoluteTime(localStartTime);
                        }
                    }
                }
                // Если количество TimePoints равно одной точке:
                else {
                    var timePoint = preset.TimePoints[0];

                    for (var i = 0; i < preset.TimerLoops[timerCycle]; ++i) {

                        nextTime = timePoint.GetAbsoluteTime(localStartTime);

                        queue.Enqueue((nextTime, timePoint));
                        localStartTime = timePoint.GetAbsoluteTime(localStartTime);
                    }
                }
            }

            return queue;
        }

        /// <summary>
        /// Gets preset index
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetIndex(string name)
        {
            if (Presets != null)
                for (int i = 0; i < Presets.Count; ++i) {
                    if (name.Equals(Presets[i].PresetName, StringComparison.OrdinalIgnoreCase))
                        return i;
                }

            return -1;
        }

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
            if (_queue.Peek().Item2.Time < TimeSpan.Zero) {

                // Если цикл не бесконечный:
                if (_isInfiniteLoop == 0) {

                    Stop();
                    return;
                }

                OnChangeTimePoint(_prevQueueElement.Item2, _queue.Peek().Item2, LastTime(currentTime, _queue.Peek().Item1));
                return;
            }

            OnChangeTimePoint(_prevQueueElement.Item2, _queue.Peek().Item2, LastTime(currentTime, _queue.Peek().Item1));

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
