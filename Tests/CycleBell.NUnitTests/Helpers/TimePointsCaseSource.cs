using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;
using NUnit.Framework;

namespace CycleBell.NUnitTests.Helpers
{
    [TestFixture]
    public static class TimePointsCaseSource
    {
        [Test]
        public static void GetTimePoints_IndexIsValid_ReturnsTimePoints()
        {
            var timePoints = TimePointsCaseSource.GetTimePoints(0);

            Assert.That (timePoints.Any(), Is.Not.Null);
        }

        [Test]
        public static void GetTimePoints_IndexIsNotValid_Throws()
        {
            var ex = Assert.Catch<ArgumentException> (() => TimePointsCaseSource.GetTimePoints(-1));
            StringAssert.Contains (@"TimePoint set does not exist", ex.Message);
        }

        [Test]
        public static void GetTimePoints_IndexIsValid_ReturnsInstancesOfTimePointType()
        {
            var timePonts = TimePointsCaseSource.GetTimePoints (0);

            Assert.That (timePonts, Has.All.InstanceOf (typeof(TimePoint)));
        }


        private static Dictionary<int, Func<IEnumerable<TimePoint>>> _sources;

        public static IEnumerable<TimePoint> GetTimePoints (int index)
        {
            _sources = new Dictionary<int, Func<IEnumerable<TimePoint>>>();
            _sources[0] = TimePointSet0;

            if (_sources.ContainsKey (index)) {
                return _sources[index].Invoke();
            }

            throw new ArgumentException(@"TimePoint set does not exist");
        }

        private static TimePoint[] TimePointSet0()
        {
            var timePoints = new TimePoint[4];

            timePoints[0] = new TimePoint("Test TimePoint # 1", "0:02:00", TimePointType.Relative, 0);
            timePoints[1] = new TimePoint("Test TimePoint # 2", "0:02:00", TimePointType.Relative, 0);
            timePoints[2] = new TimePoint("Test TimePoint # 3", "0:02:00", TimePointType.Relative, 1);
            timePoints[3] = new TimePoint("Test TimePoint # 4", "0:02:00", TimePointType.Relative, 1);

            return timePoints;
        }
    }
}
