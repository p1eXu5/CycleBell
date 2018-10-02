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
    public class BaseTimeCalculatorTests : IStartTimeTimePointName
    {
        [Test]
        public void GetTimerQueue_PresetIsNull_NotThows()
        {
            var btc = GetBaseTimeCalculator();

            Assert.That (() => btc.GetTimerQueue(null), Throws.Nothing);
        }

        [Test]
        public void GetTimerQueue_PresetIsNull_ReturnsNull()
        {
            var btc = GetBaseTimeCalculator();

            Assert.That (() => btc.GetTimerQueue(null), Is.Null);
        }


        #region Factory

        public string StartTimeTimePointName => "Test Start TimePoint";

        private BaseTimeCalculator GetBaseTimeCalculator()
        {
            var btc = new BaseTimeCalculator (this);

            return btc;
        }

        #endregion
    }
}
