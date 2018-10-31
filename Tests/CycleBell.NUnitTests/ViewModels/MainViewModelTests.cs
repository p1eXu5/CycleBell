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
// ReSharper disable AccessToModifiedClosure

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
        public void ctor__ByDefault_PresetCollectionContainsTheNewPresets__Trow()
        {
            var presets = new[]
            {
                GetDefaultPreset(),
                GetDefaultPreset("Some name"),
                GetDefaultPreset("Some name2"),
            };

            Assert.Throws(typeof(ArgumentException), () => GetMainViewModel(presets));
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

        #region SelectedPreset

        

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
        private ReadOnlyObservableCollection<Preset> _presets;

        private MainViewModel GetMainViewModel(Preset[] presets = null)
        {
            var presetColl = new ObservableCollection<Preset>();
            _presets = new ReadOnlyObservableCollection<Preset>(presetColl);

            _mockTimerManager = _mockCycleBellManager.As<ITimerManager>();
            _mockPresetCollectionManager = _mockCycleBellManager.As<IPresetCollectionManager>();

            _mockCycleBellManager.Setup (cbm => cbm.TimerManager).Returns (_mockTimerManager.Object);
            _mockCycleBellManager.Setup (cbm => cbm.PresetCollectionManager).Returns (_mockPresetCollectionManager.Object);
            _mockCycleBellManager.Setup(cbm => cbm.IsNewPreset(It.IsAny<Preset>()))
                                 .Returns((Preset preset) => PresetChecker.IsNewPreset(preset));

            _mockCycleBellManager.Setup (m => m.CreateNewPreset()).Returns(() => 
                                                    { 
                                                        presetColl.Add (Preset.GetDefaultPreset());
                                                        return true;
                                                    });


            if (presets == null) {

                presetColl = new ObservableCollection<Preset>();
                _presets = new ReadOnlyObservableCollection<Preset>(presetColl);

                _mockPresetCollectionManager.Setup(pcm => pcm.Presets)
                    .Returns(_presets);
            }
            else {
                presetColl = new ObservableCollection<Preset>(presets);
                _presets = new ReadOnlyObservableCollection<Preset>(presetColl);
                
                _mockPresetCollectionManager.Setup(pcm => pcm.Presets)
                    .Returns(_presets);
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
