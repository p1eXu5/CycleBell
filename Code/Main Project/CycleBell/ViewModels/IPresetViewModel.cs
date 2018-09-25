using System.Windows.Input;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels
{
    public interface IPresetViewModel
    {
        void UpdateSoundBank (TimePoint timePoint);
        void RemoveTimePoint (TimePoint timePoint);
        Preset Preset { get; }
        ICommand AddTimePointCommand { get; }
        bool IsRunning { get; }
    }
}