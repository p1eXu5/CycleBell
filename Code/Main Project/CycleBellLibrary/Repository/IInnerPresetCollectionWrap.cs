using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Context
{
    public interface IInnerPresetCollectionWrap : IPresetCollectionWrap
    {
        void Add(Preset preset);
        void Remove (Preset preset);
    }
}