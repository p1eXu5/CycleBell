using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Engine.Models;

namespace CycleBell.Tests.UnitTests.Factories
{
    public class PresetFactory
    {
        public static Preset GetPresetWithSounds()
        {
            var preset = new Preset("Test Preset With Sounds");
            preset.AddTimePoints( TimePointFactory.GetTimePointsWithSounds() );

            return preset;
        }
    }
}
