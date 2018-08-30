using System;
using System.Collections.Generic;
using CycleBellLibrary.Models;

namespace CycleBellLibrary.Repository
{
    public interface ITimePointBaseTimeSetter
    {
        void UpdateEachBaseTime (IEnumerable<TimePoint> timePoints, TimeSpan startTime);
        void UpdateBaseTime (TimePoint timePoint, IEnumerable<TimePoint> timePoints, TimeSpan startTime);
    }
}
