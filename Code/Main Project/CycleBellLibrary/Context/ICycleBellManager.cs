﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBellLibrary.Timer;

namespace CycleBellLibrary
{
    public interface ICycleBellManager
    {
        IPresetCollection PresetCollection { get; }
        ITimerManager TimerManager { get; }
        string FileName { get; }

        void CreateNewPreset();
        void SavePresets();
    }
}