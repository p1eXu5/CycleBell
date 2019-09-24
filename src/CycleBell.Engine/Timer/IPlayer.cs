using System;
using System.Windows;

namespace CycleBell.Engine.Timer
{
    public interface IPlayer
    {
        Uri Source { get; }
        bool HasAudio { get; }
        void Open ( Uri source );
        void Play ();
        void Stop ();

        void Close ();

        double BufferingProgress { get; }
        bool IsBuffering { get; }

        Duration NaturalDuration { get; }

        event EventHandler MediaOpened;
    }
}
