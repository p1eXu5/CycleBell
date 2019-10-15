using System;
using System.Collections.Generic;
using System.Linq;
using CycleBell.Engine.Models;
using CycleBell.Engine.Timer;
using NUnit.Framework;

namespace CycleBell.Engine.Tests.UnitTests.Timer
{
    [TestFixture]
    public class TimerQueueCalculatorTests : IStartTimePointCreator
    {
        [Test]
        public void GetTimerQueue_PresetIsNull_NotThows()
        {
            var btc = GetTimeQueueCalculator();

            Assert.That (() => btc.GetTimerQueue(null), Throws.Nothing);
        }

        [Test]
        public void GetTimerQueue_PresetIsNull_ReturnsNull()
        {
            var btc = GetTimeQueueCalculator();

            Assert.That (() => btc.GetTimerQueue(null), Is.Null);
        }

        [Test]
        public void GetTimerQueue_PresetContainsTimePoint_ReturnsExpectedTimePoints()
        {
            var btc = GetTimeQueueCalculator();
            var startTime = TimeSpan.Parse ("12:00:00");
            var preset = new Preset (new[]
                {
                    new TimePoint("Test point 1", "0:00:10", TimePointKinds.Relative, 10),
                    new TimePoint("Test point 2", "12:00:20", TimePointKinds.Absolute, 20),
                }) { StartTime = startTime };

            Queue<(TimeSpan, TimePoint)> expectedQueue = new Queue<(TimeSpan, TimePoint)>(new []
                {
                    (TimeSpan.Parse ("12:00:00"), TimerManager.GetStartTimePoint (startTime)),
                    (TimeSpan.Parse ("12:00:10"), preset.TimePointCollection[0]),
                    (TimeSpan.Parse ("12:00:20"), preset.TimePointCollection[1]),
                });

            var actualQueue = btc.GetTimerQueue (preset);

            var comparer = new TimePointComparer();

            do {
                Assert.That(expectedQueue.Dequeue().Item2, Is.EqualTo(actualQueue.Dequeue().Item2).Using<TimePoint>(comparer));

            } while (expectedQueue.Any());
        }

        [Test]
        public void GetTimerQueue_TestPresetsWithTheTestTimePoints_ReturnsExpectedNextChangeTime()
        {
            var btc = GetTimeQueueCalculator();
            var testData = GetTestPresetsAndResults();

            for (int i = 0; i < 2; ++i) {

                var actualQueue = btc.GetTimerQueue(testData.presets[i]);
                Assert.That(actualQueue.Count == testData.results[i].Length);

                for (int j = 0; j < actualQueue.Count; ++j) {

                    Assert.That(actualQueue.Peek().nextChangeTime, Is.EqualTo(testData.results[i][j]));
                    actualQueue.Enqueue(actualQueue.Dequeue());
                }
            }
        }

        [Test]
        public void GetTimerQueue__PresetContainsTimePoint_PreserveBaseTimeIsFalse__ChangesBaseTime()
        {
            var testData = GetTestPresetsAndResults();
            var preset = testData.presets[2];
            var result = testData.results[2];

            GetTimeQueueCalculator().GetTimerQueue(preset, false);

            Assert.That(preset.TimePointCollection[0].BaseTime == result[1], $"TimePoint.BaseTime:\n{preset.TimePointCollection[0]}\nExpected BaseTime:\t{result[1]}\n");
            Assert.That(preset.TimePointCollection[1].BaseTime == result[2], $"TimePoint.BaseTime:\n{preset.TimePointCollection[1]}\nExpected BaseTime:\t{result[2]}\n");
            Assert.That(preset.TimePointCollection[2].BaseTime == result[3], $"TimePoint.BaseTime:\n{preset.TimePointCollection[2]}\nExpected BaseTime:\t{result[3]}\n");
        }

        [Test]
        public void GetTimerQueue_PresetContainsTimePoint_ChangesBaseTime()
        {
            var testData = GetTestPresetsAndResults();
            var preset = testData.presets[2];
            var result = testData.results[2];

            GetTimeQueueCalculator().GetTimerQueue(preset);

            Assert.That(preset.TimePointCollection[0].BaseTime == result[4]);
            Assert.That(preset.TimePointCollection[1].BaseTime == result[5]);
            Assert.That(preset.TimePointCollection[2].BaseTime == result[6]);
        }

        [ Test ]
        public void GetTimeQueue_ByDefault_ReturnsDifferentQueues()
        {
            // Arrange:
            var calc = GetTimeQueueCalculator();

            var preset = new Preset("Test Prest") {
                StartTime = TimeSpan.Parse( "1:11:11" ),
            };
            preset.AddTimePoint( new TimePoint( "Test TimePoint", "0:01", TimePointKinds.Relative ) );

            // Action:
            var tq1 = calc.GetTimerQueue( preset );
            var tq2 = calc.GetTimerQueue( preset );

            // Assert:
            Assert.False( object.ReferenceEquals( tq1, tq2 ) );
        }

        [ Test ]
        public void GetTimeQueue_ByDefault_ReturnsEqualsQueues()
        {
            // Arrange:
            var calc = GetTimeQueueCalculator();

            var preset = new Preset("Test Prest") {
                StartTime = TimeSpan.Parse( "1:11:11" ),
            };
            preset.AddTimePoint( new TimePoint( "Test TimePoint", "0:01", TimePointKinds.Relative ) );
            
            var comparer = new TimePointComparer();

            // Action:
            var tq1 = calc.GetTimerQueue( preset );
            var tq2 = calc.GetTimerQueue( preset );

            // Assert:
            Assert.That( tq1, Is.EquivalentTo( tq2 ).Using< TimePoint >( comparer ) );
        }


        [ Test ]
        public void GetTimeQueue_AfterChangeStartTime_ReturnsNotEqualQueues()
        {
            // Arrange:
            var calc = GetTimeQueueCalculator();

            var preset = new Preset("Test Prest") {
                StartTime = TimeSpan.Parse( "1:11:11" ),
            };
            preset.AddTimePoint( new TimePoint( "Test TimePoint", "0:01", TimePointKinds.Relative ) );

            var comparer = new TimePointComparer();

            // Action:
            var tq1 = calc.GetTimerQueue( preset );

            preset.StartTime = TimeSpan.Parse( "1:11:12" );
            var tq2 = calc.GetTimerQueue( preset );

            // Assert:
            Assert.That( tq1, Is.Not.EquivalentTo( tq2 ).Using< TimePoint >( comparer ) );
        }


        [ Test ]
        public void GetTimeQueue_AfterChangeStartTime_FirstTimePointInQueuesIsStartTimePoint()
        {
            // Arrange:
            var calc = GetTimeQueueCalculator();

            var preset = new Preset("Test Prest") {
                StartTime = TimeSpan.Parse( "1:11:11" ),
            };
            preset.AddTimePoint( new TimePoint( "Test TimePoint", "0:01", TimePointKinds.Relative ) );

            var comparer = new TimePointComparer();

            // Action:
            var tq1 = calc.GetTimerQueue( preset );
            Assert.True( TimerManager.IsStartTimePoint( tq1.First().nextTimePoint, preset ), $"{tq1.First().nextTimePoint}" );

            // Assert:
            preset.StartTime = TimeSpan.Parse( "1:11:12" );
            var tq2 = calc.GetTimerQueue( preset );
            Assert.True( TimerManager.IsStartTimePoint( tq2.First().nextTimePoint, preset ) );
        }



        #region Factory

        public string StartTimePointName => TimerManager.START_TIMEPOINT_NAME;

        public TimePoint GetStartTimePoint(TimeSpan startTime)
        {
            return new TimePoint( StartTimePointName, startTime, TimePointKinds.Absolute );
        }



        private TimerQueueCalculator GetTimeQueueCalculator()
        {
            var btc = new TimerQueueCalculator (this);

            return btc;
        }

        private (Preset[] presets, TimeSpan[][] results) GetTestPresetsAndResults()
        {
            var presets = new Preset[3];
            var result = new TimeSpan[3][];

            #region GetTimerQueue_PresetContainsTimePoint_ReturnsExpectedTimePoints

            // 0: 
            presets[0] = new Preset (new[]
            {
                new TimePoint("0:00:00", TimePointKinds.Absolute),
                new TimePoint("0:00:00", TimePointKinds.Absolute),
                new TimePoint("0:00:00", TimePointKinds.Absolute, 1),
                new TimePoint("0:00:00", TimePointKinds.Absolute, 1),
            })
            {
                StartTime = TimeSpan.Parse("0:00:00"),
                TimerLoopDictionary = {[0] = 2, [1] = 2}
            };

            // nextChangeTimes
            result[0] = Enumerable.Range(0, 9).Select(n => TimeSpan.Parse("0:00:00")).ToArray();

            // 1:
            presets[1] = new Preset(new[]
            {
                new TimePoint("23:59:00", TimePointKinds.Absolute),
                new TimePoint("1:00:00", TimePointKinds.Relative),
                new TimePoint("23:59:00", TimePointKinds.Relative, 1),
                new TimePoint("01:00:00", TimePointKinds.Absolute, 1),
            })
            {
                StartTime = TimeSpan.Parse("0:00:00"),
                TimerLoopDictionary = { [0] = 2, [1] = 2 }
            };

            // nextChangeTimes
            result[1] = new []
            {
                TimeSpan.Parse("0:00:00"), 
                TimeSpan.Parse("23:59:00"), 
                TimeSpan.Parse("0:59:00"), 
                TimeSpan.Parse("23:59:00"), 
                TimeSpan.Parse("0:59:00"), 
                TimeSpan.Parse("0:58:00"), 
                TimeSpan.Parse("1:00:00"), 
                TimeSpan.Parse("0:59:00"), 
                TimeSpan.Parse("1:00:00"), 
            };

            #endregion

            #region GetTimerQueue_PresetContainsTimePoint_ChangesBaseTime

            // 2: 
            presets[2] = new Preset(TimeSpan.Parse("1:00:00"));

            presets[2].AddTimePoints(new[]
            {
                new TimePoint("Relative Test TimePoint 1", "1:00:00", TimePointKinds.Relative),
                new TimePoint("Absolute Test TimePoint", "23:59:00", TimePointKinds.Absolute),
                new TimePoint("Relative Test TimePoint 2", "1:00:00", TimePointKinds.Relative),
            });

            var loopNum = presets[2].TimePointCollection[0].LoopNumber;

            presets[2].TimerLoopDictionary[loopNum] = 2;

            // BaseTimes
            result[2] = new[]
            {
                TimeSpan.Parse("0:00:00"),  // [0] - start timepoiont basetime
                TimeSpan.Parse("1:00:00"),  // [1] - Relative Test TimePoint 1
                TimeSpan.Parse("2:00:00"),  // [2] - Absolute Test TimePoint
                TimeSpan.Parse("23:59:00"), // [3] - Relative Test TimePoint 2
                TimeSpan.Parse("0:59:00"),  // [4] - Relative Test TimePoint 1 (BaseTime is changed)
                TimeSpan.Parse("1:59:00"),  // [5] - Absolute Test TimePoint (BaseTime is changed)
                TimeSpan.Parse("23:59:00"), // [6] - Relative Test TimePoint 2 (BaseTime is changed)
            };

            #endregion

            return (presets, result);
        }

        #endregion
    }
}
