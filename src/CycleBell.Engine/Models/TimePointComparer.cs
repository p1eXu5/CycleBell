using System;
using System.Collections.Generic;

namespace CycleBell.Engine.Models
{
    public class TimePointComparer : EqualityComparer<TimePoint>
    {
        public override bool Equals(TimePoint x, TimePoint y)
        {
            if (x == null || y == null )
                return Object.Equals(x, y);

            return (x.Time == y.Time
                    && x.BaseTime.Equals(y.BaseTime)
                    && x.Kind == y.Kind
                    && x.LoopNumber == y.LoopNumber);
        }

        public override int GetHashCode(TimePoint obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            return obj.GetHashCode();
        }
    }
}