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
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer
{
    public class TimerQueueCalculator : ITimerQueueCalculator
    {
        private readonly IStartTimePointCreator _startTimePointCreator;

        public TimerQueueCalculator(IStartTimePointCreator startTimePointCreator)
        {
            _startTimePointCreator = startTimePointCreator ?? throw new ArgumentNullException(nameof(startTimePointCreator));
        }

        /// <summary>
        /// Creates alarm queue
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <param name="preserveBaseTime">Does TimePoint BaseTime preserve when GetAbsoluteTime methog is called?</param>
        /// <returns>The queue of tuples consists of time of the day and TimePoint that will come in this time</returns>
        public Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)> GetTimerQueue(Preset preset, bool preserveBaseTime = true)
        {
            if (preset?.TimePointCollection == null || preset.TimePointCollection.Count == 0)
                return null;

            Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)> queue = new Queue<(TimeSpan, TimePoint)>();

            TimeSpan time = preset.StartTime;
            TimePoint startTimePoint = _startTimePointCreator.GetStartTimePoint (time);

            time = startTimePoint.GetAbsoluteTime();
            queue.Enqueue((time, startTimePoint));

            foreach (var timerCycle in preset.TimerLoopDictionary.Keys) {

                if (preset.TimePointCollection.Count > 1) {

                    // Список временных точек каждого временного сегмента, порядоченный по Id (по порядку создания)
                    var timePoints = preset.TimePointCollection
                                           .Where(t => t.LoopNumber == timerCycle)
                                           .OrderBy(t => t.Id)
                                           .ToList();

                    if (timePoints.Count > 1) {
                        for (var i = 0; i < preset.TimerLoopDictionary[timerCycle]; ++i) {

                            foreach (var nextTimePoint in timePoints) {

                                time = nextTimePoint.GetAbsoluteTime (time, preserveBaseTime);
                                queue.Enqueue((time, nextTimePoint));
                            }
                        }
                    }
                    else if (timePoints.Count == 1) {
                        for (var i = 0; i < preset.TimerLoopDictionary[timerCycle]; ++i) {

                            time = timePoints[0].GetAbsoluteTime (time, preserveBaseTime);
                            queue.Enqueue((time, timePoints[0]));
                        }
                    }
                }
                else if(preset.TimePointCollection.Count == 1) {

                    var timePoint = preset.TimePointCollection[0];

                    for (var i = 0; i < preset.TimerLoopDictionary[timerCycle]; ++i) {

                        time = timePoint.GetAbsoluteTime (time, preserveBaseTime);
                        queue.Enqueue((time, timePoint));
                    }
                }
            }

            return queue;
        }
    }
}
