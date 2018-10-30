using System;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Repository.Tests
{
    [TestFixture]
    public class PresetCollectionManagerTests
    {
        [Test]
        public void AddPreset_PresetIsNull_Throws()
        {
            var presetsManager = GetPresetsManager();

            var ex = Assert.Catch<Exception> (() => presetsManager.Add (null));

            StringAssert.Contains ("preset can't be null", ex.Message);
        }

        [Test]
        public void AddPreset_ValidPreset_AddsPreset()
        {
            var presetsManager = GetPresetsManager();
            var preset = Preset.GetDefaultPreset();

            presetsManager.Add (preset);

            Assert.IsTrue (presetsManager.Presets.Count == 1);
        }

        [Test]
        public void AddPreset_ExistingPreset_NotThrows()
        {
            var presetsManager = GetPresetsManager();
            presetsManager.Add(Preset.GetDefaultPreset());

            Assert.DoesNotThrow (() => presetsManager.Add (Preset.GetDefaultPreset()));
        }

        [Test]
        public void AddPreset_PresetNameIsNull_Throws()
        {
            var pm = GetPresetsManager();
            var preset = new Preset() {PresetName = null};

            Assert.That (() => pm.Add (preset), Throws.ArgumentNullException);
        }

        [Test]
        public void Remove_ExistingPreset_RemovesPreset()
        {
            var pm = GetPresetsManager();
            pm.Add(Preset.GetDefaultPreset());
            var addedPreset = pm.Presets[0];

            pm.Remove (addedPreset);

            Assert.IsTrue (pm.Presets.Count == 0);
        }

        #region Factory

        private PresetCollectionManager GetPresetsManager()
        {
            return new PresetCollectionManager();
        }

        #endregion
    }
}
