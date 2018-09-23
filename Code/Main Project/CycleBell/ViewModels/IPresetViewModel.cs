using System.Windows.Input;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels
{
    public interface IPresetViewModel
    {
        void UpdateSoundBank (TimePoint timePoint);
        void RemoveTimePoint (TimePoint timePoint);
        ICommand AddTimePointCommand { get; }
    }
}