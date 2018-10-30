using CycleBellLibrary.Models;

namespace CycleBell.ViewModels
{
    public static class ServiceExtensions
    {
        public static bool IsNotNew(this Preset preset)
        {
            return !Preset.IsDefaultPreset(preset);
        }
    }
}