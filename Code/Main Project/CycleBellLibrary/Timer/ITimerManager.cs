﻿/*
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
using CycleBellLibrary.Models;

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
        string StartTimePointName { get; }

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