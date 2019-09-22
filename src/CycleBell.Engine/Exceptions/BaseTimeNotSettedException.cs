using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Engine.Models;

namespace CycleBell.Engine.Exceptions
{
    public class BaseTimeNotSettedException : ArgumentException
    {
        public BaseTimeNotSettedException( TimePoint timePoint)
        : base("Base time is not setted ")
        {
            TimePoint = timePoint;
        }

        public TimePoint TimePoint { get; }

        public override string Message => base.Message + TimePoint.ToString();
    }
}
