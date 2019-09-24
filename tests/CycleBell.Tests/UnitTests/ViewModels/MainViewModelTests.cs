using System;
using System.Collections.ObjectModel;
using System.Linq;
using CycleBell.Base;
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Models.Extensions;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;
using CycleBell.ViewModels;
using Moq;
using NUnit.Framework;
// ReSharper disable AccessToModifiedClosure

namespace CycleBell.Tests.UnitTests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        [TearDown]
        public void ClearPresetCollection()
        {
            _mockPresetCollectionManager?.Object.Clear();
        }


        #region class tests

        [Test]
        public void class_TypeCheck_IsObservableObject()
        {
            Type actualType = typeof(MainViewModel).BaseType;
            Type expectedType = typeof(ObservableObject);

            Assert.AreEqual(expectedType, actualType);
        } 

        #endregion


        #region ctor tests

        [Test]
        public void ctor__ByDefault_PresetCollectionContainsPresetWithoutName__Trows()
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


        #region MediaTerminalCommand tests

        [Test]
        public void MediaTerminalCommand_TimerIsStopped_CallsTimerManagerPlayAsync()
        {
            var mvm = GetMainViewModel(new [] { GetTestFilledPreset() } );

            mvm.MediaTerminalCommand.Execute(null);
           
            _mockTimerManager.Verify(a => a.PlayAsync(It.IsAny< Preset >()));
        }

        #endregion



        #region Factory

        private readonly Mock< IDialogRegistrator > _mockDialogRegistrator = new Mock< IDialogRegistrator >();
        private readonly Mock< ICycleBellManager > _mockCycleBellManager = new Mock< ICycleBellManager >();
        private Mock< ITimerManager > _mockTimerManager;
        private Mock< IPresetCollection > _mockPresetCollectionManager;
        private ReadOnlyObservableCollection<Preset> _presets;

        private MainViewModel GetMainViewModel(Preset[] presets = null)
        {

            _mockTimerManager = _mockCycleBellManager.As<ITimerManager>();
            _mockPresetCollectionManager = _mockCycleBellManager.As< IPresetCollection >();


            ObservableCollection<Preset> presetColl;


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

            _mockCycleBellManager.Setup (m => m.CreateNewPreset()).Returns(() => 
                                                    { 
                                                        presetColl.Add (Preset.GetDefaultPreset());
                                                        return true;
                                                    });

            _mockCycleBellManager.Setup (cbm => cbm.TimerManager).Returns (_mockTimerManager.Object);

            _mockCycleBellManager.Setup (cbm => cbm.PresetCollection).Returns (_mockPresetCollectionManager.Object);

            _mockCycleBellManager.Setup(cbm => cbm.IsNewPreset(It.IsAny<Preset>()))
                                 .Returns((Preset preset) => preset.IsDefaultNamed() );

            var stubAlarm = new Mock<IAlarm>();
            var scoll = new ObservableCollection<Uri>();
            stubAlarm.Setup( a => a.DefaultSoundCollection ).Returns( new ReadOnlyObservableCollection< Uri >( scoll ) );


            var mainViewModel = new MainViewModel (_mockDialogRegistrator.Object, _mockCycleBellManager.Object, stubAlarm.Object);

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
