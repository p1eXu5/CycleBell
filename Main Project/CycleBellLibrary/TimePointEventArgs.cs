﻿using System;

namespace CycleBellLibrary
{
    public class TimePointEventArgs : EventArgs
    {
        public TimePointEventArgs(TimePoint prevTimePoint, TimePoint nextTimePoint, TimeSpan lastTime)
        {
            PrevTimePoint = prevTimePoint;
            NextTimePoint = nextTimePoint;
            LastTime = lastTime;
        }

        public TimePoint PrevTimePoint { get; }
        public TimePoint NextTimePoint { get; }
        public TimeSpan LastTime { get; }
    }
}