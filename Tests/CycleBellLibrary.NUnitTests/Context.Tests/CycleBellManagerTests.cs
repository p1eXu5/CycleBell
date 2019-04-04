using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBellLibrary.NUnitTests.Context.Tests
{
    [TestFixture]
    public class CycleBellManagerTests
    {
        [Test]
        public void CreateNewPreset_ByDefault_CreatesTheNewPreset()
        {
            // Arrange:
            var cbm = GetCycleBellManager();

            // Action:
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (cbm.PresetCollectionManager.Presets.Count
                        ,Is.GreaterThan (0));
        }

        [Test]
        public void CreateNewPreset_ByDefault_CreatesTheNewPresetEqualedDefaultPreset()
        {
            // Arrange:
            var cbm = GetCycleBellManager();
            var defaultPreset = Preset.GetDefaultPreset();
            var comparer = new EasyPresetEqualityComparer();

            // Action:
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (cbm.PresetCollectionManager.Presets[0]
                ,Is.EqualTo (defaultPreset).Using(comparer));
        }

        [Test]
        public void CreateNewPreset_NewPresetDoesntExist_CallsPresetManager()
        {
            var cbm = GetCycleBellManager();

            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetCollectionManager.Presets.Count == 1);
        }

        [Test]
        public void CreateNewPreset_NewPresetExists_DoesNotCallPresetManager()
        {
            // Arrange
            var cbm = GetCycleBellManager();
            cbm.CreateNewPreset();

            // Action
            cbm.CreateNewPreset();

            // Assert
            Assert.IsTrue(cbm.PresetCollectionManager.Presets.Count == 1); 
        }

        [Test]
        public void CreateNewPreset_NewPresetDoesntExists_DoesntRiseCantCreateNewPresetEvent()
        {
            // Arrange:
            var cbm = GetCycleBellManager();
            var isRised = false;
            cbm.CantCreateNewPresetEvent += (s, e) => { isRised = true; };

            // Action:
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (isRised, Is.EqualTo (false));
        }

        [Test]
        public void CreateNewPreset_NewPresetExistsAndIsDefalt_RiseCantCreateNewPresetEvent()
        {
            // Arrange:
            var cbm = GetCycleBellManager();
            var isRised = false;
            cbm.CantCreateNewPresetEvent += (s, e) => { isRised = true; };

            // Action:
            cbm.CreateNewPreset();
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (isRised, Is.EqualTo (true));
        }

        [Test]
        public void CreateNewPreset_NewPresetExistsAndIsDefalt_RiseEventWithEmptyPresetNotModifiedReasonFlag()
        {
            // Arrange:
            var cbm = GetCycleBellManager();
            var reason = CantCreateNewPresetReasonsEnum.UnknownReason;

            cbm.CantCreateNewPresetEvent += (s, e) => { reason = e.CantCreateNewPresetReasonEnum; };

            // Action:
            cbm.CreateNewPreset();
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (reason, Is.EqualTo (CantCreateNewPresetReasonsEnum.NewPresetNotModified));
        }

        [Test]
        public void CreateNewPreset_ExistNewPresetWithNotDefaultStartTime_RiseEventWithEmptyPresetModifiedReasonFlag()
        {
            // Arrange:
            var cbm = GetCycleBellManager();
            var reason = CantCreateNewPresetReasonsEnum.UnknownReason;

            cbm.CantCreateNewPresetEvent += (s, e) => { reason = e.CantCreateNewPresetReasonEnum; };

            // Action:
            cbm.CreateNewPreset();

            var preset = cbm.PresetCollectionManager.Presets[0];
            preset.StartTime = TimeSpan.FromSeconds (7352930);

            cbm.CreateNewPreset();

            // Assert:
            Assert.That (reason, Is.EqualTo (CantCreateNewPresetReasonsEnum.NewPresetModified));
        }

        [Test]
        public void CreateNewPreset_ExistNewPresetWithNotDefaultTimePointCollection_RiseEventWithEmptyPresetModifiedReasonFlag()
        {
            // Arrange:
            var cbm = GetCycleBellManager();
            var reason = CantCreateNewPresetReasonsEnum.UnknownReason;

            cbm.CantCreateNewPresetEvent += (s, e) => { reason = e.CantCreateNewPresetReasonEnum; };

            // Action:
            cbm.CreateNewPreset();

            var preset = cbm.PresetCollectionManager.Presets[0];
            preset.AddTimePoint (new TimePoint(TimeSpan.FromSeconds (7352930)));

            cbm.CreateNewPreset();

            // Assert:
            Assert.That (reason, Is.EqualTo (CantCreateNewPresetReasonsEnum.NewPresetModified));
        }

        [Test]
        public void AddPreset_WhenCalled_CallsPresetManager()
        {
            var cbm = GetCycleBellManager();
            var addingPreset = Preset.GetDefaultPreset();
            
            cbm.AddPreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollectionManager.Presets.Count == 1);
        }

        [Test]
        public void RemovePreset_WhenCalled_CallsPresetsManager()
        {
            var cbm = GetCycleBellManager();
            var addingPreset = Preset.GetDefaultPreset();
            
            cbm.AddPreset (addingPreset);

            cbm.RemovePreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollectionManager.Presets.Count == 0);
        }

        [Test]
        public void IsNewPreset_DefaulPreset_ReturnsTrue()
        {
            var preset = new Preset();
            var cbm = GetCycleBellManager();

            Assert.That(cbm.IsNewPreset(preset), Is.EqualTo(true));
        }

        [Test]
        public void IsNewPreset_NamedPreset_ReturnsFalse()
        {
            var preset = new Preset("asd");
            var cbm = GetCycleBellManager();

            Assert.That(cbm.IsNewPreset(preset), Is.EqualTo(false));
        }

        
        #region Factory

        private Mock<IInnerPresetCollectionManager> _mockPresetCollectionManager;

        private CycleBellManager GetCycleBellManager()
        {
            FakeTimerManager stubTimerManager = new FakeTimerManager();

            _mockPresetCollectionManager = new Mock<IInnerPresetCollectionManager>();

            List<Preset> presetList = new List<Preset>();

            _mockPresetCollectionManager.Setup (m => m.Add (It.IsAny<Preset>())).Callback ((Preset p) => presetList.Add (p));
            _mockPresetCollectionManager.Setup (m => m.Presets).Returns (() => new ReadOnlyObservableCollection<Preset>(new ObservableCollection<Preset>(presetList)));
            _mockPresetCollectionManager.Setup (m => m.Remove (It.IsAny<Preset>())).Callback ((Preset p) => presetList.Remove (p));

            return new CycleBellManager (_mockPresetCollectionManager.Object, stubTimerManager);
        }

        #endregion

        #region FakeTypes

        #pragma warning disable 0067
        internal class FakeTimerManager : ITimerManager
        {
            public event EventHandler<TimerEventArgs> TimePointChanged;
            public event EventHandler<TimerEventArgs> TimerSecondPassedEvent;
            public event EventHandler TimerStarted;
            public event EventHandler TimerPauseEvent;
            public event EventHandler TimerStopEvent;
            public bool IsRunning { get; } = false;
            public bool IsPaused { get; } = false;
            public string StartTimePointName { get; } = null;

            public void Pause()
            {
                throw new NotImplementedException();
            }

            public void Resume()
            {
                throw new NotImplementedException();
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }

            public void PlayAsync (Preset preset)
            {
                throw new NotImplementedException();
            }

            public void Play (Preset preset)
            {
                throw new NotImplementedException();
            }

            public void DontPreserveBaseTime()
            {}

            public void PreserveBaseTime()
            {}
        }
        #pragma warning restore 0067

        #endregion

        public class EasyPresetEqualityComparer : IEqualityComparer<Preset>
        {
            public bool Equals (Preset x, Preset y)
            {
                if (x == null) return y == null;

                return x.PresetName.Equals (y.PresetName, StringComparison.InvariantCulture)
                       && x.StartTime.Equals (y.StartTime)
                       && x.IsInfiniteLoop.Equals (y.IsInfiniteLoop)
                       && Object.Equals (x.Tag, y.Tag)
                       && x.TimePointCollection.Count.Equals (y.TimePointCollection.Count)
                       && x.TimerLoops.Count.Equals (y.TimerLoops.Count);
            }

            public int GetHashCode (Preset obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }
    }
}
