using System;

namespace CycleBell.Engine.Models
{
    public class LoopNumberChangedEventArgs : EventArgs
    {
        public LoopNumberChangedEventArgs( int newLoopNumber, int oldLoopNumber )
        {
            NewLoopNumber = newLoopNumber;
            OldLoopNumber = oldLoopNumber;
        }

        public int OldLoopNumber { get; }
        public int NewLoopNumber { get; }
    }
}
