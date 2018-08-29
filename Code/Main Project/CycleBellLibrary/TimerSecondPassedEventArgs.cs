using System;

namespace CycleBellLibrary
{
    public class TimerSecondPassedEventArgs : EventArgs
    {
        public TimerSecondPassedEventArgs(TimeSpan lastTime)
        {
            LastTime = lastTime;
        }

        public TimeSpan LastTime { get; }
    }
}