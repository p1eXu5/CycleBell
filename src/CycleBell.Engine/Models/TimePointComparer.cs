using System;
using System.Collections.Generic;

namespace CycleBell.Engine.Models
{
    /// <summary>
    /// Compares two time points by fields.
    /// </summary>
    public class TimePointComparer : EqualityComparer<TimePoint>
    {
        /// <summary>
        /// Create comparer that compares two time points by fields.
        /// </summary>
        public TimePointComparer():base(){}

        /// <inheritdoc cref="TimePointComparer"/>
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