using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    public interface IPresetsManager
    {
        event NotifyCollectionChangedEventHandler CollectionChanged;

        string FileName { get; set; }
        ReadOnlyObservableCollection<Preset> Presets { get; }

        void Clear();
        void LoadFromFile(string fileName);
        void Add(Preset preset);
        void SavePresets();
    }
}
