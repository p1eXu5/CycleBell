using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Views;
using NUnit.Framework;

namespace CycleBell.NUnitTests.View
{
    [TestFixture]
    public class TimeSpanDigitTests
    {
        [TestCase ("-9:59:41", '-', null, 9, 5, 9, 4, 1, 0)]
        [TestCase ("9:59:41", null, null, 9, 5, 9, 4, 1, 0)]
        [TestCase ("-19:59:41", '-', 1, 9, 5, 9, 4, 1, 0)]
        [TestCase ("19:59:41", null, 1, 9, 5, 9, 4, 1, 0)]
        public void Parse_NegateTimeSpan_Parses (string time, char? sign, byte? majorH, byte minorH, byte majorM, byte minorM, byte majorS, byte minorS, int milliseconds)
        {
            var timeSpan = TimeSpan.Parse ((time));

            var digits = TimeSpanDigits.Parse (timeSpan);

            Assert.AreEqual (sign, digits.Sign);
            Assert.AreEqual (majorH, digits.MajorH);
            Assert.AreEqual (minorH, digits.MinorH);
            Assert.AreEqual (majorM, digits.MajorM);
            Assert.AreEqual (minorM, digits.MinorM);
            Assert.AreEqual (majorS, digits.MajorS);
            Assert.AreEqual (minorS, digits.MinorS);
            Assert.AreEqual (milliseconds, digits.Milliseconds);
        }
    }
}
