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
 *
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Timer
{
    public class TimerQueueCalculator : ITimerQueueCalculator
    {
        private readonly IStartTimeTimePointName _startTimeTimePointName;

        public TimerQueueCalculator(IStartTimeTimePointName startTimeTimePointName)
        {
            _startTimeTimePointName = startTimeTimePointName ?? throw new ArgumentNullException(nameof(startTimeTimePointName));
        }

         /// <summary>
        /// Creates alarm queue
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <returns>The queue of tuples consists of time of the day and TimePoint that will come in this time</returns>
        public Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)> GetTimerQueue(Preset preset)
         {
            if (preset?.TimePointCollection == null || preset.TimePointCollection.Count == 0)
                return null;

            // Смещение по времени следующей временной точки
            TimeSpan startTime = preset.StartTime;

            // Очередь кортежей времени будильника и соответствующей ему NextTimePoint
            Queue<(TimeSpan, TimePoint)> queue = new Queue<(TimeSpan, TimePoint)>();

            queue.Enqueue((startTime, new TimePoint(_startTimeTimePointName.StartTimeTimePointName, startTime, TimePointType.Absolute){BaseTime = startTime}));

            // Заполняем очередь

            // Для всех временных сегментов
            foreach (var timerCycle in preset.TimerLoops.Keys) {

                TimeSpan nextTime;

                if (preset.TimePointCollection.Count > 1) {

                    // Список временных точек каждого временного сегмента, порядоченный по Id (по порядку создания)
                    var timePoints = preset.TimePointCollection.Where(t => t.LoopNumber == timerCycle).OrderBy(t => t.Id)
                                           .ToList();

                    for (var i = 0; i < preset.TimerLoops[timerCycle]; ++i) {

                        foreach (var point in timePoints) {

                            nextTime = point.GetAbsoluteTime(startTime);

                            queue.Enqueue((nextTime, point));
                            startTime = point.GetAbsoluteTime(startTime);
                        }
                    }
                }
                else {

                    // If TimePointCollection.Count == 1
                    var timePoint = preset.TimePointCollection[0];

                    for (var i = 0; i < preset.TimerLoops[timerCycle]; ++i) {

                        nextTime = timePoint.GetAbsoluteTime(startTime);

                        queue.Enqueue((nextTime, timePoint));
                        startTime = timePoint.GetAbsoluteTime(startTime);
                    }
                }
            }

            return queue;
        }
    }
}
