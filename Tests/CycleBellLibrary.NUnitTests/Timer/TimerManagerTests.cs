using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Timer
{
    [TestFixture]
    public class TimerManagerTests
    {
        #region Play

        [Test]
        public void Play_PresetIsNull_EmitsStopEvent()
        {
            var tm = GetTimerManager();

            bool res = false;
            tm.TimerStopEvent += (sender, args) => res = true;

            tm.Play (null);

            Assert.AreEqual (true, res);
        }

        [Test]
        public void Play_PresetDoesNotContainAnyTimePoints_EmitsStopEvent()
        {
            var tm = GetTimerManager();
            var preset = new Preset();

            bool res = false;
            tm.TimerStopEvent += (sender, args) => res = true;

            tm.Play (preset);

            Assert.AreEqual (true, res);
        }

        [Test]
        public void Play_PresetIsNull_DoesNotEmitStartEvent()
        {
            var tm = GetTimerManager();

            bool res = false;
            tm.TimerStartEvent += (sender, args) => res = true;

            tm.Play (null);

            Assert.AreNotEqual (true, res);
        }

        [Test]
        public void Play_PresetDoesNotContainAnyTimePoints_DoesNotEmitStartEvent()
        {
            var tm = GetTimerManager();
            var preset = new Preset();

            bool res = false;
            tm.TimerStartEvent += (sender, args) => res = true;

            tm.Play (preset);

            Assert.AreNotEqual (true, res);
        }

        #endregion



        #region Factory

        private TimerManager GetTimerManager()
        {
            var tm = TimerManager.Instance;
            return tm;
        }

        #endregion
    }
}
