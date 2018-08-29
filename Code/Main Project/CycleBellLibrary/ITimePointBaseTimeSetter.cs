using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleBellLibrary
{
    public interface ITimePointBaseTimeSetter
    {
        void UpdateEachBaseTime (IEnumerable<TimePoint> timePoints, TimeSpan startTime);
        void UpdateBaseTime (TimePoint timePoint, IEnumerable<TimePoint> timePoints, TimeSpan startTime);
    }
}
