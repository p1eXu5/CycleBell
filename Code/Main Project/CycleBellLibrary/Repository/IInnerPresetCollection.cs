using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Context
{
    public interface IInnerPresetCollection : IPresetCollection
    {
        void Add(Preset preset);
        void Remove (Preset preset);
    }
}