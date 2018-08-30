﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Context.Tests
{
    [TestFixture]
    public class PresetsManagerTests
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
            var preset = Preset.EmptyPreset;

            presetsManager.Add (preset);

            Assert.IsTrue (presetsManager.Presets.Count == 1);
        }

        [Test]
        public void AddPreset_ExistingPreset_Throws()
        {
            var presetsManager = GetPresetsManager();
            presetsManager.Add(Preset.EmptyPreset);

            var ex = Assert.Catch<Exception> (() => presetsManager.Add (Preset.EmptyPreset));

            StringAssert.Contains ("preset already exists", ex.Message);
        }

        [Test]
        public void Remove_ExistingPreset_RemovesPreset()
        {
            var pm = GetPresetsManager();
            pm.Add(Preset.EmptyPreset);
            var addedPreset = pm.Presets[0];

            pm.Remove (addedPreset);

            Assert.IsTrue (pm.Presets.Count == 0);
        }

        #region Factory

        private PresetsManager GetPresetsManager()
        {
            return new PresetsManager();
        }

        #endregion
    }
}
