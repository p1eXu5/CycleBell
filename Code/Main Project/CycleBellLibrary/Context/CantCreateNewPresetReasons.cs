using System;

namespace CycleBellLibrary.Context {
    [Flags]
    public enum CantCreateNewPresetReasonsFlags
    {
        EmptyPresetNotModified = 0x0,
        EmptyPresetModified = 0x2,
        UnknownReason = 0x128
    }
}