using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer 
{
    public interface IAlarm
    {
        ReadOnlyObservableCollection< Uri > DefaultSounds { get; }

        void LoadDefaultSounds();

        bool SetDefaultSound ( Uri uri );
        void AddSound ( TimePoint tPoint );
        void LoadNextSound ( TimePoint timePoint );
        void Play ();
        void PlayDefault ();
        void Stop ();
        void StopDefault ();
    }
}