using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;
using CycleBell.Base;
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Models.Extensions;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;
using CycleBell.Tests.UnitTests.Factories;
using CycleBell.ViewModels;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
// ReSharper disable AccessToModifiedClosure

namespace CycleBell.Tests.UnitTests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        #region test setup

        //[ SetUp ]
        //public void SetupForTests()
        //{

        //}

        private void TestWithNoRegistryKeys( Action assertion )
        {
            string defaultSound = "", selectedPreset = "";
            var existingKey = Registry.CurrentUser.OpenSubKey( MainViewModel.REG_PATH );
            
            if ( existingKey != null ) 
            {
                defaultSound = existingKey.GetValue( MainViewModel.DEFAULT_SOUND_KEY )?.ToString() ?? "";
                selectedPreset = existingKey.GetValue( MainViewModel.DEFAULT_SOUND_KEY )?.ToString() ?? "";

                existingKey.Close();

                Registry.CurrentUser.DeleteSubKey( MainViewModel.REG_PATH );
            }

            assertion();

            if ( existingKey != null ) 
            {
                var key = Registry.CurrentUser.CreateSubKey( MainViewModel.REG_PATH );
                
                key.SetValue( "Default Sound", defaultSound );
                key.SetValue( "Selected Preset", selectedPreset );

                key.Close();
            }
        }

        private void TestWithRegistryKeys( Action assertion, string selectedPreset = "", string sound = "" )
        {
            string defaultSound = AppDomain.CurrentDomain.BaseDirectory + sound,
                lastSavedPreset = "", lastDefaultSound = "";

            var existingKey = Registry.CurrentUser.OpenSubKey( MainViewModel.REG_PATH, true );
            
            if ( existingKey != null ) 
            {
                lastSavedPreset = existingKey.GetValue( MainViewModel.SELECTED_PRESET_KEY )?.ToString() ?? "";
                lastDefaultSound = existingKey.GetValue( MainViewModel.DEFAULT_SOUND_KEY )?.ToString() ?? "";

                existingKey.SetValue( MainViewModel.DEFAULT_SOUND_KEY, defaultSound );
                existingKey.SetValue( MainViewModel.SELECTED_PRESET_KEY, selectedPreset );

                existingKey.Close();
            }
            else {
                var key = Registry.CurrentUser.CreateSubKey( MainViewModel.REG_PATH );
                key.SetValue( MainViewModel.DEFAULT_SOUND_KEY, defaultSound );
                key.SetValue( MainViewModel.SELECTED_PRESET_KEY, selectedPreset );
                key.Close();
            }
                


            assertion();


            
            if ( existingKey != null ) 
            {
                existingKey = Registry.CurrentUser.OpenSubKey( MainViewModel.REG_PATH, true );
                existingKey.SetValue( MainViewModel.SELECTED_PRESET_KEY, lastSavedPreset );
                existingKey.SetValue( MainViewModel.DEFAULT_SOUND_KEY, lastDefaultSound );
                existingKey.Close();
            }
            else {
                Registry.CurrentUser.DeleteSubKey( MainViewModel.REG_PATH );
            }
        }

        [TearDown]
        public void ClearPresetCollection()
        {
            _mockPresetCollectionManager?.Object.Clear();
        }

        #endregion


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

            mvm.PlayPauseCommand.Execute(null);
           
            _mockTimerManager.Verify(a => a.PlayAsync(It.IsAny< Preset >()));
        }

        #endregion


        #region LoadUserSettingsCommand tests

        [ Test ]
        public void LoadUserSettingsCommand__PresetsEmpty__CannotExecute()
        {
            var mvm = GetMainViewModel( null );

            Assert.IsFalse( mvm.LoadUserSettingsCommand.CanExecute( null ) );
        }

        [ Test ]
        public void LoadUserSettingsCommand__PresetsNotEmpty_SettingsIsNotSaved__CannotExecute()
        {
            // Arrange:
            // Action:
            var mvm = GetMainViewModel( PresetFactory.GetPresetCollection() );

            // Assert:
            TestWithNoRegistryKeys( () => {

                Assert.IsFalse( mvm.LoadUserSettingsCommand.CanExecute( null ) );
            } );
        }

        [ Test ]
        public void LoadUserSettingsCommand__SettingsIsSaved__CallsAlarmSetDefaultSound()
        {
            // Arrange:
            // Action:
            var mvm = GetMainViewModel( PresetFactory.GetPresetCollection() );

            // Assert:
            TestWithRegistryKeys( () => {
                mvm.LoadUserSettingsCommand.Execute( null );
                _mockAlarm.Verify( a => a.SetDefaultSound( It.IsAny< Uri >() ), Times.Once() );
            } );
        }

        [ Test ]
        public void LoadUserSettingsCommand__SettingsIsSaved_SavedPresetExists__SetsSelectedPreset()
        {
            // Arrange:
            var presets = PresetFactory.GetPresetCollection();
            // Action:
            var mvm = GetMainViewModel( presets );

            // Assert:
            TestWithRegistryKeys( () => {

                mvm.LoadUserSettingsCommand.Execute( null );
                Assert.That( mvm.SelectedPreset.Name, Is.EqualTo( presets[1].PresetName ) );
            }, presets[1].PresetName );
        }

        [ Test ]
        public void LoadUserSettingsCommand__SettingsIsSaved_SavedPresetDoesNotExist__DoesNotSetSelectedPreset()
        {
            // Arrange:
            var presets = PresetFactory.GetPresetCollection();
            var notExistName = "Not Exist Name";

            // Action:
            var mvm = GetMainViewModel( presets );

            // Assert:
            TestWithRegistryKeys( () => {

                mvm.LoadUserSettingsCommand.Execute( null );
                Assert.That( mvm.SelectedPreset.Name, Is.Not.EqualTo( notExistName ) );
            }, notExistName );
        }

        #endregion


        #region SaveUserSettingsCommand tests

        [ Test ]
        public void SaveUserSettingsCommand_SettingsIsNotSaved_SaveSettings()
        {
            var mvm = GetMainViewModel( PresetFactory.GetPresetCollection() );
            mvm.SelectedPreset = mvm.PresetViewModelCollection[ 1 ];


            TestWithNoRegistryKeys( () => {

                mvm.SaveUserSettingsCommand.Execute(null);

                var key = Registry.CurrentUser.OpenSubKey( MainViewModel.REG_PATH );

                Assert.IsNotNull( key );
                Assert.That( key.GetValue( MainViewModel.SELECTED_PRESET_KEY ).ToString(), Is.EqualTo( mvm.PresetViewModelCollection[1].Name ) );

                var ds = key.GetValue( MainViewModel.DEFAULT_SOUND_KEY ).ToString();

                key.Close();

                Registry.CurrentUser.DeleteSubKey( MainViewModel.REG_PATH );
            } );
        }


        [ Test ]
        public void SaveUserSettingsCommand_SettingsIsNotSaved_CallsAlarmGetDefaultSound()
        {
            var mvm = GetMainViewModel( PresetFactory.GetPresetCollection() );
            mvm.SelectedPreset = mvm.PresetViewModelCollection[ 1 ];


            TestWithNoRegistryKeys( () => {

                mvm.SaveUserSettingsCommand.Execute(null);

                _mockAlarm.Verify( a => a.GetDefaultSound(), Times.Once );

                Registry.CurrentUser.DeleteSubKey( MainViewModel.REG_PATH );
            } );
        }


        [ Test ]
        public void SaveUserSettingsCommand_SettingsIsSaved_OverridesSettings()
        {
            var mvm = GetMainViewModel( PresetFactory.GetPresetCollection() );
            mvm.SelectedPreset = mvm.PresetViewModelCollection[ 1 ];
            
            var sound = new Uri( AppDomain.CurrentDomain.BaseDirectory + "\\Sounds\\Alarm 2.mp3" );
            mvm.Alarm.SetDefaultSound( sound );


            TestWithRegistryKeys( () => {

                mvm.SaveUserSettingsCommand.Execute(null);

                var key = Registry.CurrentUser.OpenSubKey( MainViewModel.REG_PATH );

                Assert.IsNotNull( key );
                Assert.That( key.GetValue( MainViewModel.SELECTED_PRESET_KEY ).ToString(), Is.EqualTo( mvm.PresetViewModelCollection[1].Name ) );
                Assert.That( key.GetValue( MainViewModel.DEFAULT_SOUND_KEY ).ToString(), Is.EqualTo( sound.ToString() ) );

                key.Close();
            } );
        }

        #endregion



        #region Factory

        private readonly Mock< IDialogRegistrator > _mockDialogRegistrator = new Mock< IDialogRegistrator >();
        private readonly Mock< ICycleBellManager > _mockCycleBellManager = new Mock< ICycleBellManager >();
        private Mock< ITimerManager > _mockTimerManager;
        private Mock< IPresetCollection > _mockPresetCollectionManager;
        private Mock< IAlarm > _mockAlarm;
        private string _defaultSound;
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

            _mockAlarm = new Mock<IAlarm>();
            var scoll = new ObservableCollection<Uri>();
            _mockAlarm.Setup( a => a.DefaultSoundCollection ).Returns( new ReadOnlyObservableCollection< Uri >( scoll ) );
            _mockAlarm.Setup( a => a.SetDefaultSound( It.IsAny< Uri >() ) ).Callback< Uri >( u => _defaultSound = u.OriginalString );
            _mockAlarm.Setup( a => a.GetDefaultSound() ).Returns( () => _defaultSound != null ? new Uri( _defaultSound ) : new Uri( "c:\\test.test" ) );


            var mainViewModel = new MainViewModel (_mockDialogRegistrator.Object, _mockCycleBellManager.Object, _mockAlarm.Object);

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
