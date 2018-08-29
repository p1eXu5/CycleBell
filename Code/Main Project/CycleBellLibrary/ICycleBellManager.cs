using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    public interface ICycleBellManager
    {
        ICollection<Preset> Presets { get; }

        event NotifyCollectionChangedEventHandler PresetCollectionChanged;

        void CreateNewPreset();
    }
}
