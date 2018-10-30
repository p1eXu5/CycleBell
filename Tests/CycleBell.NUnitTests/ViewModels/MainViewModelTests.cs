using System;
using System.Collections.ObjectModel;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;

namespace CycleBell.NUnitTests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        [Test]
        public void class_TypeCheck_IsObservableObject()
        {
            Type actualType = typeof(MainViewModel).BaseType;
            Type expectedType = typeof(ObservableObject);

            Assert.AreEqual(expectedType, actualType);
        }

        #region ctor

        [Test]
        public void ctor__ByDefault_PresetCollectionContainsOnlyDefaultNamedPresets__CreatesEmptyTimePointViewModelCollection()
        {
            var presets = new[]
            {
                GetDefaultPreset(),
                GetDefaultPreset()
            };
            
            var mvm = GetMainViewModel(presets);

            Assert.That(mvm.PresetViewModelCollection.Count, Is.EqualTo(0));
        }

        [Test]
        public void ctor__ByDefault_PresetCollectionContainsNotOnlyDefaultNamedPresets__CreatesNotEmptyTimePointViewModelCollection()
        {
            var presets = new[]
            {
                GetDefaultPreset(),
                GetDefaultPreset("Some name"),
                GetDefaultPreset("Some name2"),
            };

            var mvm = GetMainViewModel(presets);

            Assert.That(mvm.PresetViewModelCollection.Count, Is.EqualTo(2));

            _mockPresetCollectionManager.Object.Clear();
        }

        #endregion

        #region PlayCommand

        [Test]
        public void MediaTerminal_TimerStateIsTrue_CallsTimerManagerPlayAsync()
        {
            var mvm = GetMainViewModel(new [] {GetTestFilledPreset()});

            mvm.MediaTerminalCommand.Execute(null);
           
            _mockTimerManager.Verify(a => a.PlayAsync(It.IsAny<Preset>()));
        }

        #endregion

        [TearDown]
        public void ClearPresetCollection()
        {
            _mockPresetCollectionManager?.Object.Clear();
        }

        #region Factory

        private readonly Mock<IDialogRegistrator> _mockDialogRegistrator = new Mock<IDialogRegistrator>();
        private readonly Mock<ICycleBellManager> _mockCycleBellManager = new Mock<ICycleBellManager>();
        private Mock<ITimerManager> _mockTimerManager;
        private Mock<IPresetCollectionManager> _mockPresetCollectionManager;

        private MainViewModel GetMainViewModel(Preset[] presets = null)
        {
            _mockTimerManager = _mockCycleBellManager.As<ITimerManager>();
            _mockPresetCollectionManager = _mockCycleBellManager.As<IPresetCollectionManager>();

            _mockCycleBellManager.Setup (cbm => cbm.TimerManager).Returns (_mockTimerManager.Object);
            _mockCycleBellManager.Setup (cbm => cbm.PresetCollectionManager).Returns (_mockPresetCollectionManager.Object);
            _mockCycleBellManager.Setup(cbm => cbm.IsNewPreset(It.IsAny<Preset>()))
                                 .Returns((Preset preset) => CycleBellManager.PresetChecker.IsNewPreset(preset));

            if (presets == null) {
                _mockPresetCollectionManager.Setup(pcm => pcm.Presets)
                                            .Returns(new ReadOnlyObservableCollection<Preset>(new ObservableCollection<Preset>()));
            }
            else {
                _mockPresetCollectionManager.Setup(pcm => pcm.Presets)
                                            .Returns(new ReadOnlyObservableCollection<Preset>(new ObservableCollection<Preset>(presets)));
            }

            var mainViewModel = new MainViewModel (_mockDialogRegistrator.Object, _mockCycleBellManager.Object);

            return mainViewModel;
        }

        private Preset GetDefaultPreset(string name = null) => new Preset(name);

        private Preset GetTestFilledPreset()
        {
            var preset = new Preset("Test Preset");

            preset.AddTimePoint(TimePoint.GetAbsoluteTimePoint());

            return preset;
        }

        #endregion
    }
}
