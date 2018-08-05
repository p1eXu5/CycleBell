using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace CycleBell.Models
{
    public enum TimePointType : byte
    {
        Absolute = 1,
        Duration
    }

    public class TimePoint
    {
        private SoundPlayer _sound;

        #region Constructors

        public TimePoint() : this(new SoundPlayer()) {}
        public TimePoint(SoundPlayer sound) => _sound = sound;
        public TimePoint(TimeSpan ts, SoundPlayer sp) : this(sp) => Time = ts;
        public TimePoint(TimeSpan ts) : this(new SoundPlayer()) => Time = ts;

        #endregion

        public SoundPlayer Sound { get; set; }
        public TimeSpan Time { get; set; } = DateTime.Now.TimeOfDay + TimeSpan.FromMinutes(5);
        public string Name { get; set; } = "Unnamed";
        public byte TimeSection { get; set; } = 0;
    }
}
