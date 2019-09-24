using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using CycleBell.Base;
using CycleBell.ViewModels;
using Moq;
using NUnit.Framework;
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Models.Extensions;
using CycleBell.Engine.Repository;
using CycleBell.Engine.Timer;

// ReSharper disable AccessToModifiedClosure

namespace CycleBell.Tests.FunctionalTests.ViewModels
{
    [TestFixture, RequiresThread(ApartmentState.STA)]
    public class MainViewModelIntegrationalTests
    {
        [Test]
        public void CreateNewPreset__SelectedPresetHasNotANewPreset_NewPresetDoesntExist__SetsSelectedPresetEqualedToANewPreset()
        {
            // Arrange:
            var mvm = GetMainViewModel();
            mvm.CreateNewPresetCommand.Execute (null);
            mvm.SelectedPreset.Preset.PresetName = "Some Name";

            // Action:
            mvm.CreateNewPresetCommand.Execute (null);

            // Assert:
            Assert.IsTrue(mvm.SelectedPreset.Preset.IsDefaultNamed());
        }

        [Test]
        public void CreateNewPreset__SelectedPresetHasNotANewPreset_NewPresetExists__SetsSelectedPresetEqualedToANewPreset()
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
            Assert.IsTrue(mvm.SelectedPreset.Preset.IsDefaultNamed());
        }

        [Test]
        public void SelectedPresetSet__SelectedPresetHasANewPresetAndSettedToANotNewPreset__DontRiseOnCantCreateNewPresetEvent()
        {
            // Arrange:
            var mvm = GetMainViewModel();
            mvm.CreateNewPresetCommand.Execute(null);

            mvm.SelectedPreset.Preset.PresetName = "Some Name";
            mvm.CreateNewPresetCommand.Execute(null);

            bool isRised = false;
            _cycleBellManager.CantCreateNewPresetEvent += (s, e) => { isRised = true; };

            // Action:
            mvm.SelectedPreset = mvm.PresetViewModelCollection[0];

            // Assert:
            Assert.That(isRised, Is.EqualTo(false));
        }

        [Test]
        public void SelectedPresetSet__SelectedPresetHasANewAndModifiedPreset_SettedToANotNewPreset__CallsDialog()
        {
            // Arrange:
            var mvm = GetMainViewModel();
            mvm.CreateNewPresetCommand.Execute(null);
            mvm.SelectedPreset.Preset.PresetName = "Some Name";

            mvm.CreateNewPresetCommand.Execute(null);
            mvm.SelectedPreset.Preset.StartTime = TimeSpan.FromSeconds(1234567890);

            // Action:
            mvm.SelectedPreset = mvm.PresetViewModelCollection[0];

            // Assert:
            _mockDialogRegistrator.Verify(d => d.ShowDialog(It.IsAny<IDialogCloseRequested>(), It.IsAny<Predicate<object>>()));
        }

        #region Factory

        private readonly Mock<IDialogRegistrator> _mockDialogRegistrator = new Mock<IDialogRegistrator>();

        private ICycleBellManager _cycleBellManager;
        //private readonly Mock<ICycleBellManager> _mockCycleBellManager = new Mock<ICycleBellManager>();

        private PresetCollection _presetCollectionManager;

        private Mock<ITimerManager> _mockTimerManager;

        private MainViewModel GetMainViewModel(Preset[] presets = null)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) {
                throw new ThreadStateException("The current threads apartment state is not STA");
            }

            _mockTimerManager = new Mock<ITimerManager>();

            _presetCollectionManager = new PresetCollection();

            try {
                _cycleBellManager = new CycleBellManager("some_file_name", _presetCollectionManager, _mockTimerManager.Object);
            }
            catch (ArgumentNullException) { }

            var stubAlarm = new Mock<IAlarm>();
            var scoll = new ObservableCollection<Uri>();
            stubAlarm.Setup( a => a.DefaultSoundCollection ).Returns( new ReadOnlyObservableCollection< Uri >( scoll ) );

            var mainViewModel = _cycleBellManager != null 
                                    ? new MainViewModel (_mockDialogRegistrator.Object, _cycleBellManager, stubAlarm.Object ) 
                                    : throw new ArgumentNullException(nameof(_cycleBellManager), @"Instance of ICycleBellManager is null");

            Window wnd = new FakeMainWindow();
            wnd.DataContext = mainViewModel;

            return mainViewModel;
        }

        private Preset GetDefaultPreset(string name = null) => new Preset(name);

        private Preset GetTestFilledPreset()
        {
            var preset = new Preset("Test Preset");

            preset.AddTimePoint(TimePoint.GetAbsoluteTimePoint());

            return preset;
        }

        private class FakeMainWindow : Window
        {

            public FakeMainWindow()
            {
                var dock = new DockPanel();
                dock.LastChildFill = true;

                var menu = new Menu();
                menu.Height= Double.NaN;
                menu.Background = Brushes.White;
                DockPanel.SetDock(menu, Dock.Top);

                var statusBar = new StatusBar();
                statusBar.Background = Brushes.Transparent;
                statusBar.BorderThickness = new Thickness(0);
                statusBar.FontSize = 10;
                DockPanel.SetDock(statusBar, Dock.Bottom);

                var textBlock = new TextBlock();
                var statusBarBinding = new Binding();
                statusBarBinding.Path = new PropertyPath("StatusBarText");
                statusBarBinding.Mode = BindingMode.OneWay;
                statusBarBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                textBlock.SetBinding(TextBlock.TextProperty, statusBarBinding);

                var statusBarItem = new StatusBarItem();
                statusBarItem.Content = textBlock;
                statusBar.Items.Add(statusBarItem);

                var presetsGrid = new Grid();
                presetsGrid.VerticalAlignment = VerticalAlignment.Stretch;
                TextElement.SetFontSize(presetsGrid, 14.0);

                var presets = new ComboBox();

                var collBinding = new Binding();
                collBinding.Path = new PropertyPath("PresetViewModelCollection");
                collBinding.Mode = BindingMode.OneWay;

                presets.SetBinding(ItemsControl.ItemsSourceProperty, collBinding);

                var selBinding = new Binding();
                selBinding.Path = new PropertyPath("SelectedPreset");
                selBinding.Mode = BindingMode.TwoWay;
                selBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                presets.SetBinding(Selector.SelectedItemProperty, selBinding);

                var textBinding = new Binding();
                textBinding.Path = new PropertyPath("SelectedPresetName");
                textBinding.Mode = BindingMode.TwoWay;
                textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                presets.SetBinding(ComboBox.TextProperty, textBinding);

                presetsGrid.Children.Add(presets);

                dock.Children.Add(presetsGrid);

                Content = dock;
            }
        }

        #endregion
    }
}
