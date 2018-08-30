namespace CycleBellLibrary
{
    public interface IInnerPresetsManager : IPresetsManager
    {
        void Add(Preset preset);
        void Remove (Preset preset);
    }
}