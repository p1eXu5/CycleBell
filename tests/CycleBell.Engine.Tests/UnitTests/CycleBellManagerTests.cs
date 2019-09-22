using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBell.Engine.Tests.UnitTests
{
    [TestFixture]
    public class CycleBellManagerTests
    {
        [Test]
        public void CreateNewPreset_ByDefault_CreatesTheNewPreset()
        {
            // Arrange:
            var cbm = GetMockedCycleBellManager();

            // Action:
            cbm.CreateNewPreset();

            // Assert:
            var res = cbm.PresetCollection.Count;
            Assert.That (res
                        ,Is.GreaterThan (0));
        }

        [Test]
        public void CreateNewPreset_ByDefault_CreatesTheNewPresetEqualedDefaultPreset()
        {
            // Arrange:
            var cbm = GetMockedCycleBellManager();
            var defaultPreset = Preset.GetDefaultPreset();
            var comparer = new EasyPresetEqualityComparer();

            // Action:
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (cbm.PresetCollection.Presets[0]
                ,Is.EqualTo (defaultPreset).Using(comparer));
        }

        [Test]
        public void CreateNewPreset_NewPresetDoesntExist_CallsPresetManager()
        {
            var cbm = GetMockedCycleBellManager();

            cbm.CreateNewPreset();

            Assert.IsTrue(cbm.PresetCollection.Count == 1);
        }

        [Test]
        public void CreateNewPreset_NewPresetExists_DoesNotCallPresetManager()
        {
            // Arrange
            var cbm = GetMockedCycleBellManager();
            cbm.CreateNewPreset();

            // Action
            cbm.CreateNewPreset();

            // Assert
            Assert.IsTrue(cbm.PresetCollection.Count == 1); 
        }

        [Test]
        public void CreateNewPreset_NewPresetDoesntExists_DoesntRiseCantCreateNewPresetEvent()
        {
            // Arrange:
            var cbm = GetMockedCycleBellManager();
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
            var cbm = GetMockedCycleBellManager();
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
            var cbm = GetMockedCycleBellManager();
            var reason = CantCreateNewPresetReasons.UnknownReason;

            cbm.CantCreateNewPresetEvent += (s, e) => { reason = e.CantCreateNewPresetReason; };

            // Action:
            cbm.CreateNewPreset();
            cbm.CreateNewPreset();

            // Assert:
            Assert.That (reason, Is.EqualTo (CantCreateNewPresetReasons.NewPresetNotModified));
        }

        [Test]
        public void CreateNewPreset_ExistNewPresetWithNotDefaultStartTime_RiseEventWithEmptyPresetModifiedReasonFlag()
        {
            // Arrange:
            var cbm = GetMockedCycleBellManager();
            var reason = CantCreateNewPresetReasons.UnknownReason;

            cbm.CantCreateNewPresetEvent += (s, e) => { reason = e.CantCreateNewPresetReason; };

            // Action:
            cbm.CreateNewPreset();

            var preset = cbm.PresetCollection[0];
            preset.StartTime = TimeSpan.FromSeconds (7352930);

            cbm.CreateNewPreset();

            // Assert:
            Assert.That (reason, Is.EqualTo (CantCreateNewPresetReasons.NewPresetModified));
        }

        [Test]
        public void CreateNewPreset_ExistNewPresetWithNotDefaultTimePointCollection_RiseEventWithEmptyPresetModifiedReasonFlag()
        {
            // Arrange:
            var cbm = GetMockedCycleBellManager();
            var reason = CantCreateNewPresetReasons.UnknownReason;

            cbm.CantCreateNewPresetEvent += (s, e) => { reason = e.CantCreateNewPresetReason; };

            // Action:
            cbm.CreateNewPreset();

            var preset = cbm.PresetCollection.Presets[0];
            preset.AddTimePoint (new TimePoint(TimeSpan.FromSeconds (7352930)));

            cbm.CreateNewPreset();

            // Assert:
            Assert.That (reason, Is.EqualTo (CantCreateNewPresetReasons.NewPresetModified));
        }

        [Test]
        public void AddPreset_WhenCalled_CallsPresetManager()
        {
            var cbm = GetMockedCycleBellManager();
            var addingPreset = Preset.GetDefaultPreset();
            
            cbm.AddPreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollection.Presets.Count == 1);
        }

        [Test]
        public void RemovePreset_WhenCalled_CallsPresetsManager()
        {
            var cbm = GetMockedCycleBellManager();
            var addingPreset = Preset.GetDefaultPreset();
            
            cbm.AddPreset (addingPreset);

            cbm.RemovePreset (addingPreset);

            Assert.IsTrue (cbm.PresetCollection.Presets.Count == 0);
        }

        [Test]
        public void IsNewPreset_DefaulPreset_ReturnsTrue()
        {
            var preset = new Preset();
            var cbm = GetMockedCycleBellManager();

            Assert.That(cbm.IsNewPreset(preset), Is.EqualTo(true));
        }

        [Test]
        public void IsNewPreset_NamedPreset_ReturnsFalse()
        {
            var preset = new Preset("asd");
            var cbm = GetMockedCycleBellManager();

            Assert.That(cbm.IsNewPreset(preset), Is.EqualTo(false));
        }

        
        #region Factory

        private Mock<IPresetCollection> _mockPresetCollection;

        private CycleBellManager GetMockedCycleBellManager()
        {
            FakeTimerManager stubTimerManager = new FakeTimerManager();

            _mockPresetCollection = new Mock<IPresetCollection>();

            List<Preset> presetList = new List<Preset>();

            _mockPresetCollection.Setup (m => m.Add (It.IsAny<Preset>())).Callback ((Preset p) => presetList.Add (p));
            _mockPresetCollection.Setup (m => m.Presets).Returns (() => new ReadOnlyObservableCollection<Preset>(new ObservableCollection<Preset>(presetList)));
            _mockPresetCollection.Setup (m => m.Remove (It.IsAny<Preset>())).Callback ((Preset p) => presetList.Remove (p));
            _mockPresetCollection.Setup (m => m.Count).Returns( () => presetList.Count );
            _mockPresetCollection.Setup (m => m[It.IsAny<int>()]).Returns( (int index) => presetList[index] );

            return new CycleBellManager (_mockPresetCollection.Object, stubTimerManager);
        }

        #endregion

        #region FakeTypes

        #pragma warning disable 0067
        internal class FakeTimerManager : ITimerManager
        {
            public event EventHandler<TimerEventArgs> TimePointChanged;
            public event EventHandler<TimerEventArgs> SecondPassed;
            public event EventHandler TimerStarted;
            public event EventHandler TimerPaused;
            public event EventHandler TimerStopped;
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
                       && x.TimerLoopDictionary.Count.Equals (y.TimerLoopDictionary.Count);
            }

            public int GetHashCode (Preset obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }
    }
}
