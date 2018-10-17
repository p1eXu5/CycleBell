﻿using System;
using System.Collections.Generic;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Timer
{
    public interface ITimerManager
    {
        event EventHandler<TimerEventArgs> ChangeTimePointEvent;
        event EventHandler<TimerEventArgs> TimerSecondPassedEvent;
        event EventHandler TimerStartEvent;
        event EventHandler TimerPauseEvent;
        event EventHandler TimerStopEvent;

        bool IsRunning { get; }
        bool IsPaused { get; }
        string StartTimeTimePointName { get; }

        /// <summary>
        /// Pause timer loop
        /// </summary>
        void Pause();

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


    }
}