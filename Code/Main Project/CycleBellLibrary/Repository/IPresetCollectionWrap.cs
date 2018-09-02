using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Context
{
    public interface IPresetCollectionWrap : IEnumerable<Preset>
    {
        ReadOnlyObservableCollection<Preset> Presets { get; }

        void Clear();
        void LoadFromFile(string fileName);
        void SavePresets(string fileName);
    }
}
