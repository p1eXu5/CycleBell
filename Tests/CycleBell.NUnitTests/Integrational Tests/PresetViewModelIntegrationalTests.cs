using System;
using CycleBellLibrary.Models;
using NUnit.Framework;

namespace CycleBell.NUnitTests.Integrational_Tests
{
    [TestFixture]
    class PresetViewModelIntegrationalTests
    {
        #region Factory

        private Preset GetPreset(TimeSpan startTime) => new Preset(new []
        {
            new TimePoint("Test TimePoint # 1", "0:01:00"),
            new TimePoint("Test TimePoint # 2", "0:01:00"),
        }) { StartTime = startTime };


        #endregion
    }
}
