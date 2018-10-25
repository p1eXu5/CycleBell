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
        public void MediaTerminal_TimerStateIsTrue_CallsTimerManager()
        {
            var mvm = GetMainViewModel();
           
        }

        #endregion

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

        #endregion
    }
}
