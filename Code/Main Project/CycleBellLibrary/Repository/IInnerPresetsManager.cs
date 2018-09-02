using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Context
{
    public interface IInnerPresetsManager : IPresetsManager
    {
        void Add(Preset preset);
        void Remove (Preset preset);
    }
}