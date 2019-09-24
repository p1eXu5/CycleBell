using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Timer 
{
    public interface IAlarm
    {
        string DefaultSoundsDirrectory { get; set; }

        ReadOnlyObservableCollection< Uri > DefaultSoundCollection { get; }
        IEnumerable< KeyValuePair< int, Uri > > TimePointSoundDictionary { get; }

        bool CanPlay { get; }
        bool CanPlayDefault { get;}

        void LoadDefaultSoundCollection();

        void SetDefaultSound();
        void SetDefaultSound ( Uri uri );
        void AddSound ( TimePoint tPoint );
        void RemoveSound ( TimePoint tPoint );
        void LoadNextSound ( TimePoint timePoint );
        void Play ();
        void PlayDefault ();
        void Stop ();
        void StopDefault ();

        void Reset();
    }
}