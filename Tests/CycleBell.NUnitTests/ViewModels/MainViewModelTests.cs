using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBell.ViewModels.TimePointViewModels;
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

        #region PlayCommand

        [Test]
        public void PlayCommand_SelectedPresetIsNull_CannotExecute()
        {
            var pvm = GetMainViewModel();

            Assert.IsFalse (pvm.PlayCommand.CanExecute (null));
        }

        [Test]
        public void PlayCommand_SelectedPresetIsExistsTimerIsNotRunning_CanExecute()
        {
            var mvm = GetMainViewModel();

            mvm.SelectedPreset = GetPresetViewModel(mvm);
            _mockTimerManager.Setup (tm => tm.IsRunning).Returns (false);

            Assert.IsTrue (mvm.PlayCommand.CanExecute(null));
        }

        [Test]
        public void PlayCommand_WhenExecuted_CallsITimerManager()
        {
            var pvm = GetMainViewModel();
            pvm.CreateNewPresetCommand.Execute (null);

            pvm.PlayCommand.Execute (null);

            _mockTimerManager.Verify(tm => tm.Play (It.IsAny<Preset>()));
        }

        #endregion

        // PouseCommand

        [Test]
        public void PouseCommand_TimerIsNotRunning_CanNotExecuted()
        {
            var pvm = GetMainViewModel();

            _mockTimerManager.Setup (tm => tm.IsRunning).Returns (false);

            Assert.IsFalse (pvm.PauseCommand.CanExecute (null));
        }

        [Test]
        public void PouseCommand_TimerIsRunning_CanExecuted()
        {
            var pvm = GetMainViewModel();

            _mockTimerManager.Setup (tm => tm.IsRunning).Returns (true);

            Assert.IsTrue (pvm.PauseCommand.CanExecute(null));
        }

        [Test]
        public void PouseCommand_WhenExecuted_CallsITimerManager()
        {
            var pvm = GetMainViewModel();

            pvm.PauseCommand.Execute (null);

            _mockTimerManager.Verify (tm => tm.Pause());
        }

        #region Factory

        private readonly Mock<IDialogRegistrator> _mockDialogRegistrator = new Mock<IDialogRegistrator>();
        private readonly Mock<ICycleBellManager> _mockCycleBellManager = new Mock<ICycleBellManager>();
        private Mock<ITimerManager> _mockTimerManager;
        private Mock<IPresetCollectionManager> _mockPresetCollectionManager;

        private MainViewModel GetMainViewModel()
        {
            _mockTimerManager = _mockCycleBellManager.As<ITimerManager>();
            _mockPresetCollectionManager = _mockCycleBellManager.As<IPresetCollectionManager>();

            _mockCycleBellManager.Setup (cbm => cbm.TimerManager).Returns (_mockTimerManager.Object);
            _mockCycleBellManager.Setup (cbm => cbm.PresetCollectionManager).Returns (_mockPresetCollectionManager.Object);

            _mockPresetCollectionManager.Setup (pcm => pcm.Presets).Returns (new ReadOnlyObservableCollection<Preset>(new ObservableCollection<Preset>()));

            var mainViewModel = new MainViewModel (_mockDialogRegistrator.Object, _mockCycleBellManager.Object);

            return mainViewModel;
        }

        private PresetViewModel GetPresetViewModel(MainViewModel mainViewModel)
        {
            var preset = new Preset ("Test Preset");
            var timePoint = new TimePoint("0:00:01");
            preset.AddTimePoint (timePoint);

            var tpvm = new PresetViewModel(preset, mainViewModel);

            return tpvm;
        }

        #endregion
    }
}
