using CycleBellLibrary.Models;

namespace CycleBellLibrary.Context
{
    public static class PresetChecker
    {
        public static bool IsNewPreset(Preset preset)
        {
            if (preset == null) return false;

            return preset.PresetName == Preset.DefaultName;
        }

        public static bool IsModifiedPreset(Preset preset)
        {
            if (preset == null) return false;

            return preset.StartTime != Preset.DefaultStartTime
                    || preset.TimePointCollection.Count != 0;
        }
    }

}