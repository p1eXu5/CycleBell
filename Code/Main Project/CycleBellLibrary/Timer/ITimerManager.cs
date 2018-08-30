using System;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Timer
{
    public interface ITimerManager
    {
        event EventHandler<TimerEventArgs> ChangeTimePointEvent;
        event EventHandler<TimerEventArgs> TimerSecondPassedEvent;
        event EventHandler TimerStopEvent;

        bool IsRunning { get; }

        /// <summary>
        /// Pause timer loop
        /// </summary>
        void Pouse();

        /// <summary>
        /// Resume timer loop
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the timer
        /// </summary>
        void Stop();

        void PlayAsync(Preset preset);

        /// <summary>
        /// Запуск
        /// </summary>
        /// <param name="preset">Запускаемый пресет</param>
        void Play(Preset preset);

        /// <summary>
        /// Gets preset index
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int GetIndex(string name);
    }
}