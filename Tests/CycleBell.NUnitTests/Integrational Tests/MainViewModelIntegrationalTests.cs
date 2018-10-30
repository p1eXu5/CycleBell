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
// ReSharper disable AccessToModifiedClosure

namespace CycleBell.NUnitTests.Integrational_Tests
{
    [TestFixture]
    public class MainViewModelIntegrationalTests
    {
        [Test]
        public void CreateNewPreset__SelectedPresetNotANewPreset_NewPresetDoesntExist__SetsSelectedPresetEqualedToANewPreset()
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

        [Test]
        public void CreateNewPreset__SelectedPresetNotANewPreset_NewPresetExists__SetsSelectedPresetEqualedToANewPreset()
        {
            // Arrange:
            var mvm = GetMainViewModel();
            mvm.CreateNewPresetCommand.Execute (null);
            mvm.SelectedPreset.Preset.PresetName = "Some Name";
            mvm.CreateNewPresetCommand.Execute (null);
            mvm.SelectedPreset = mvm.PresetViewModelCollection[0];

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
            var obscol = new ObservableCollection<Preset>();
            var rObscol = new ReadOnlyObservableCollection<Preset>(obscol);

            _mockTimerManager = _mockCycleBellManager.As<ITimerManager>();
            _mockPresetCollectionManager = _mockCycleBellManager.As<IPresetCollectionManager>();

            var pcm = new PresetCollectionManager();
            var cbm = new CycleBellManager (null, pcm, _mockTimerManager.Object);

            var mainViewModel = new MainViewModel (_mockDialogRegistrator.Object, cbm);

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
