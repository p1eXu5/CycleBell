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
 *
 *
 */

using System;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer
{
    // LastTimeToNextChange - duration of a PrevTimePoint
    public class TimerEventArgs : EventArgs
    {
        public TimerEventArgs(TimePoint prevTimePoint, TimePoint nextTimePoint, TimeSpan lastTimeToNextChange, TimeSpan? prevTimePointNextBaseTime)
        {
            PrevTimePoint = prevTimePoint;
            NextTimePoint = nextTimePoint;
            LastTimeToNextChange = lastTimeToNextChange;
            PrevTimePointNextBaseTime = prevTimePointNextBaseTime;
        }

        public TimePoint PrevTimePoint { get; }
        public TimePoint NextTimePoint { get; }
        public TimeSpan LastTimeToNextChange { get; }
        public TimeSpan? PrevTimePointNextBaseTime { get; }
    }
}