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
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer
{
    using TimePointQueue =  Queue<(TimeSpan nextChangeTime, TimePoint nextTimePoint)>;

    public interface ITimerQueueCalculator
    {
        /// <summary>
        /// Creates alarm queue consisted of tuples of time of the day and TimePoint,
        /// including <see cref="IStartTimePointCreator.GetStartTimePoint"/>.
        /// </summary>
        /// <param name="preset">Preset</param>
        /// <param name="preserveBaseTime">
        /// Does TimePoint BaseTime preserve when GetAbsoluteTime methog is called?
        /// true - by default.
        /// </param>
        /// <returns>The queue of time points</returns>
        TimePointQueue GetTimerQueue( Preset preset, bool preserveBaseTime = true );
    }
}
