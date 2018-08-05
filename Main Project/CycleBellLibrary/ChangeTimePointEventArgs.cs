using System;

namespace CycleBellLibrary
{
    public class ChangeTimePointEventArgs : EventArgs
    {
        public ChangeTimePointEventArgs(TimePoint timePoint, TimeSpan lastTime)
        {
            TimePoint = timePoint;
            LastTime = lastTime;
        }

        public TimePoint TimePoint { get; }
        public TimeSpan LastTime { get; }
    }
}