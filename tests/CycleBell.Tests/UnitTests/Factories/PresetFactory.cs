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

        public static Preset[] GetPresetCollection()
        {
            return new[] {
                new Preset( "Test Preset #1" ),
                new Preset( "Test Preset #2" ),
                new Preset( "Test Preset #3" ),
            };
        }
    }
}
