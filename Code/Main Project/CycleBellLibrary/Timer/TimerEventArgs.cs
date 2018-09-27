/*
 *
 * LastTime - duration of a PrevTimePoint
 */

using System;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Timer
{
    public class TimerEventArgs : EventArgs
    {
        public TimerEventArgs(TimePoint prevTimePoint, TimePoint nextTimePoint, TimeSpan lastTime)
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