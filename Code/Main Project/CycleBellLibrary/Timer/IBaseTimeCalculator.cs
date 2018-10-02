using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBellLibrary.Timer
{
    public interface IBaseTimeCalculator
    {
        Queue<(TimeSpan, TimePoint)> GetTimerQueue (Preset _preset);
    }
}
