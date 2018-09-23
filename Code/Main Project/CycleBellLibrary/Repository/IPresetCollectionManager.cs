using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CycleBellLibrary.Repository
{
    public interface IPresetCollectionManager : IEnumerable<Preset>
    {
        ReadOnlyObservableCollection<Preset> Presets { get; }

        void Clear();
        void OpenPresets(string fileName);
        void SavePresets(string fileName);
    }
}
