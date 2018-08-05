using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    /// <summary>
    /// Посредник, менеджер.
    /// Дай мыне дирехтора!
    /// </summary>
    public class CycleBellTimerManager
    {
        public const string RestartString = "Restart";

        #region Fields

        /// <summary>
        /// Instance of class
        /// </summary>
        private static CycleBellTimerManager _cycleBellTimerManager;

        /// <summary>
        /// Дирехтор
        /// </summary>
        private readonly IDirector _director;

        /// <summary>
        /// The main queue. The next (TimeSpan startTimeForNextStartPoint, TimePoint nextPoint) always on the top.
        /// </summary>
        private Queue<(TimeSpan, TimePoint)> _queue = null;

        /// <summary>
        /// Internel timer
        /// </summary>
        private Timer _timer;

        private byte _isRunning = 0;
        private TimeSpan _deltaTime;
        private byte _isInfiniteLoop;

        /// <summary>
        /// Previous queue element
        /// </summary>
        private (TimeSpan, TimePoint) _prevQueueElement;

        #endregion

        #region Constructor

        private CycleBellTimerManager(IDirector director)
        {
            // Устанавливаем дирехтора
            _director = director;

            // Заводим коллекцию
            _director.LoadPresets();
        }

        /// <summary>
        /// Gets instance of manager and if IDirector.FileName is Exist loads presets or loads only one empty preset
        /// </summary>
        /// <param name="director"></param>
        /// <returns></returns>
        public static CycleBellTimerManager Instance(IDirector director) 
            => _cycleBellTimerManager ?? (_cycleBellTimerManager = new CycleBellTimerManager(director));
        
        #endregion

        #region Events

        public event EventHandler<TimePointEventArgs> ChangeTimePointEvent;

        public virtual void OnChangeTimePoint(TimePoint prevTimePoint, TimePoint nextTimePoint, TimeSpan lastTime)
        {
            ChangeTimePointEvent?.Invoke(this, new TimePointEventArgs(prevTimePoint, nextTimePoint, lastTime));
        }

        public event EventHandler<TimePointEventArgs> TimerSecondPassedEvent;

        public virtual void OnTimerSecondPassed(TimePoint nextTimePoint, TimeSpan lastTime)
        {
            TimerSecondPassedEvent?.Invoke(this, new TimePointEventArgs(null, nextTimePoint, lastTime));
        }

        public event EventHandler TimerStopEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnTimerStop()
        {
            TimerStopEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public static int Accuracy { get; set; } =300;

        public ReadOnlyObservableCollection<Preset> Presets => _director.Presets;
        public bool IsRunning => _isRunning != 0;

        #endregion

        #region Methods

        /// <summary>
        /// dueTime for _timer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDueTime (int i) => Accuracy - (i % Accuracy);

        /// <summary>
        /// Invoked by App exit event
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public void OnAppExit(object s, EventArgs e)
        {
            // TODO serialization:

            _director.SavePresets();

            Console.WriteLine ("Program ending");
        }

        /// <summary>
        /// Add preset to inner director's collection
        /// </summary>
        /// <param name="preset"></param>
        public void AddPreset(Preset preset) => _director.AddPreset(preset);

        public void ReloadPresets() => _director.LoadPresets();

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

            OnChangeTimePoint(_prevQueueElement.Item2, foundedNextQueueElem.Item2, LastTime(ref currentTime, foundedNextQueueElem.Item1));

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

            // Заполняем очередь таймеров
            _queue = GetTimerQueue(preset);

            //#region Print Queue

            //foreach (var point in _queue) {

            //    Console.WriteLine($"{point.Item1:h\\:mm\\:ss} {point.Item2?.Name ?? "StartTime"} \n({point.Item2})");
            //}

            //Console.WriteLine("");

            //#endregion


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

            // Сигналим о смене поинтов
            _prevQueueElement = (currentTime, new TimePoint("Launch Time", TimeSpan.FromMinutes(-1), TimePointType.Absolute));
            OnChangeTimePoint(_prevQueueElement.Item2, _queue.Peek().Item2, LastTime(ref currentTime, _queue.Peek().Item1));
        }

        /// <summary>
        /// Creates alarm queue
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <returns>Alarm queue</returns>
        public Queue<(TimeSpan, TimePoint)> GetTimerQueue(Preset preset)
        {
            if (preset?.TimePoints == null || preset.TimePoints.Count == 0)
                return null;

            // Очередь кортежей времени будильника и соответствующей ему NextTimePoint
            Queue<(TimeSpan, TimePoint)> queue = new Queue<(TimeSpan, TimePoint)>();

            // Смещение по времени следующей временной точки
            TimeSpan localStartTime = preset.StartTime;
            queue.Enqueue((localStartTime, new TimePoint("Start Time Point", localStartTime, TimePointType.Absolute)));

            // Заполняем очередь

            // Для всех временных сегментов
            foreach (var timerCycle in preset.TimersCycles.Keys) {

                TimeSpan nextTime;

                if (preset.TimePoints.Count > 1) {

                    // Список временных точек каждого временного сегмента, порядоченный по Id (по порядку создания)
                    var timePoints = preset.TimePoints.Where(t => t.TimerCycleNum == timerCycle).OrderBy(t => t.Id)
                                           .ToList();

                    for (var i = 0; i < preset.TimersCycles[timerCycle]; ++i) {

                        foreach (var point in timePoints) {

                            nextTime = point.GetAbsoluteTime(localStartTime);

                            queue.Enqueue((nextTime, point));
                            localStartTime = point.GetAbsoluteTime(localStartTime);
                        };
                    };
                }
                // Если количество TimePoints равно одной точке:
                else {
                    var timePoint = preset.TimePoints[0];

                    for (var i = 0; i < preset.TimersCycles[timerCycle]; ++i) {

                        nextTime = timePoint.GetAbsoluteTime(localStartTime);

                        queue.Enqueue((nextTime, timePoint));
                        localStartTime = timePoint.GetAbsoluteTime(localStartTime);
                    };
                }
            }

            return queue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preset"></param>
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

                OnChangeTimePoint(_prevQueueElement.Item2, _queue.Peek().Item2, LastTime(ref currentTime, _queue.Peek().Item1));
                return;
            }

            OnChangeTimePoint(_prevQueueElement.Item2, _queue.Peek().Item2, LastTime(ref currentTime, _queue.Peek().Item1));

            // Если время следующей точки равно предыдущей:
            if (_queue.Peek().Item1 == _prevQueueElement.Item1) {

                ChangeTimePoint(ref currentTime);
                return;
            }

            _deltaTime = -TimeSpan.FromHours(1);
        }

        /// <summary>
        /// Calculate last time
        /// </summary>
        /// <param name="currentTime">will be changed to lastTime</param>
        /// <param name="nextTime"></param>
        /// <returns>Last time to nextTime</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TimeSpan LastTime(ref TimeSpan currentTime, TimeSpan nextTime)
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
        public static void RunTimer (this Preset preset, CycleBellTimerManager manager)
        {
            manager.PlayAsync (preset);
        }
    }

    #endregion
}
