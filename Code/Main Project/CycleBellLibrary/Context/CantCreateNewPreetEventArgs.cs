using System;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Context
{
    public class CantCreateNewPreetEventArgs : EventArgs
    {
        public CantCreateNewPreetEventArgs(CantCreateNewPresetReasonsFlags cantCreateNewPresetReasonFlags, Preset preset)
        {
            Preset = preset ?? throw new ArgumentNullException();
            CantCreateNewPresetReasonFlags = cantCreateNewPresetReasonFlags;
        }

        public CantCreateNewPresetReasonsFlags CantCreateNewPresetReasonFlags { get; }
        public Preset Preset { get; }
    }
}