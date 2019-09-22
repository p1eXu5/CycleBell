using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer {
    public interface IAlarm {
        IPlayer Player { get; }
        IPlayer DefaultPlayer { get; }

        void SetDefaultSound ( string path );
        void AddSound ( TimePoint tPoint );
        void LoadSound ( TimePoint tPoint );
        void Play ();
        void PlayDefault ();
        void Stop ();
        void StopDefault ();
    }
}