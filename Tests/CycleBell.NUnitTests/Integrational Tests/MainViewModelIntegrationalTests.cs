using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CycleBell.NUnitTests.Integrational_Tests
{
    [TestFixture]
    public class MainViewModelIntegrationalTests
    {
        [Test]
        public void CreateNewPreset_SelectedPresetNotANewPreset_SetsSelectedPresetEqualedToANewPreset()
        {
            // Arrange:
            var mvm = GetMainViewModel();
            mvm.CreateNewPresetCommand.Execute (null);
            mvm.SelectedPreset.Preset.PresetName = "Some Name";

            // Action:
            mvm.CreateNewPresetCommand.Execute (null);

            // Assert:
            Assert.That (CycleBellManager.PresetChecker.IsNewPreset (mvm.SelectedPreset.Preset), Is.EqualTo (true));
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
