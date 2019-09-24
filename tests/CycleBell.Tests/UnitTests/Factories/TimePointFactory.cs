using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Engine.Models;

namespace CycleBell.Tests.UnitTests.Factories
{
    public class TimePointFactory
    {
        public static IEnumerable< TimePoint > GetTimePointsWithSounds()
        {
            var baseTime = DateTime.Now.TimeOfDay + TimeSpan.FromSeconds( 5 );
            var i = 1;

            yield return new TimePoint( name: "Test TimpePoint With Sound #1",
                                        kind: TimePointKinds.Absolute,
                                        time: baseTime + TimeSpan.FromSeconds( 5 * i++ ) ) {
                Tag = AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 1.mp3"
            };

            yield return new TimePoint( name: "Test TimpePoint With Sound #2",
                                        kind: TimePointKinds.Absolute,
                                        time: baseTime + TimeSpan.FromSeconds( 5 * i++ ) );

            yield return new TimePoint( name: "Test TimpePoint With Sound #3",
                                        kind: TimePointKinds.Absolute,
                                        time: baseTime + TimeSpan.FromSeconds( 5 * i ) ) {
                Tag = AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 2.mp3"
            };
        }
    }
}
