using System.Collections.Generic;
using System.ComponentModel;
using System.Media;

namespace CycleBell.ViewModels
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        IDictionary<int,SoundPlayer> SoundMap { get; }
        void Ring (int id);
        bool IsRunning { get; }
        string StartTimeName { get; }
    }
}