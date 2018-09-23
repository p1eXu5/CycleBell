using System.Collections.Generic;
using System.Media;

namespace CycleBell.ViewModels
{
    public interface IMainViewModel
    {
        IDictionary<int,SoundPlayer> SoundMap { get; }
        void Ring (int id);
    }
}