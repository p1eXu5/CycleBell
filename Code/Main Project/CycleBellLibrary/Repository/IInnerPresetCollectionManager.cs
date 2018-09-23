namespace CycleBellLibrary.Repository
{
    public interface IInnerPresetCollectionManager : IPresetCollectionManager
    {
        void Add(Preset preset);
        void Remove (Preset preset);
    }
}