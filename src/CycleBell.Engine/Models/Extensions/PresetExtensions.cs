
namespace CycleBell.Engine.Models.Extensions
{
    public static class PresetExtensions
    {
        public static bool IsDefaultNamed(this Preset preset)
        {
            if (preset == null) return false;

            return preset.PresetName == Preset.DefaultName;
        }

        public static bool IsModifiedPreset(this Preset preset)
        {
            if (preset == null) return false;

            return preset.StartTime != Preset.DefaultStartTime
                    || preset.TimePointCollection.Count != 0;
        }
    }

}