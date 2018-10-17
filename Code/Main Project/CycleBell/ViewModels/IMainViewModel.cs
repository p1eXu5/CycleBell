using System.Collections.Generic;
using System.ComponentModel;
using System.Media;

namespace CycleBell.ViewModels
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        void Ring (bool stopping = false);
        bool IsRunning { get; }
        bool IsRingOnStartTime { get; }
        string StartTimeName { get; }
    }
}