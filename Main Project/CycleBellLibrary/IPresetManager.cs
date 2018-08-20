using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    public interface IPresetManager
    {
        event NotifyCollectionChangedEventHandler CollectionChanged;

        string FileName { get; set; }
        ReadOnlyObservableCollection<Preset> Presets { get; }

        void LoadPresets();
        void AddPreset(Preset preset);
        void SavePresets();
    }
}
