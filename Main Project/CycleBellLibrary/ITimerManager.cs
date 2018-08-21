using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CycleBellLibrary
{
    public interface ITimerManager
    {
        event NotifyCollectionChangedEventHandler PresetCollectionChanged;
        event EventHandler<TimePointEventArgs> ChangeTimePointEvent;
        event EventHandler<TimePointEventArgs> TimerSecondPassedEvent;
        event EventHandler TimerStopEvent;
        ReadOnlyObservableCollection<Preset> Presets { get; }
        bool IsRunning { get; }


        /// <summary>
        /// Add preset to inner presetsManager's collection
        /// </summary>
        /// <param name="preset"></param>
        void AddPreset(Preset preset);

        void ReloadPresets();

        /// <summary>
        /// Pause timer loop
        /// </summary>
        void Pouse();

        /// <summary>
        /// Resume timer loop
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the timer
        /// </summary>
        void Stop();

        void PlayAsync(Preset preset);

        /// <summary>
        /// Запуск
        /// </summary>
        /// <param name="preset">Запускаемый пресет</param>
        void Play(Preset preset);

        /// <summary>
        /// Gets preset index
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int GetIndex(string name);
    }
}