using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    public interface IPresetsManager
    {
        ReadOnlyObservableCollection<Preset> Presets { get; }
        string FileName { get; set; }

        void Clear();
        void LoadFromFile(string fileName);
        void SavePresets();
    }
}
