using System;
using System.Collections.ObjectModel;
using System.Threading;
using CycleBell.Base;
using CycleBell.ViewModels;
using CycleBellLibrary.Context;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Moq;
using NUnit.Framework;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

// ReSharper disable AccessToModifiedClosure

namespace CycleBell.NUnitTests.Integrational_Tests
{
    [TestFixture, RequiresThread(ApartmentState.STA)]
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
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) {
                throw new ThreadStateException("The current threads apartment state is not STA");
            }

            var obscol = new ObservableCollection<Preset>();
            var rObscol = new ReadOnlyObservableCollection<Preset>(obscol);

            _mockTimerManager = _mockCycleBellManager.As<ITimerManager>();
            _mockPresetCollectionManager = _mockCycleBellManager.As<IPresetCollectionManager>();


            var pcm = new PresetCollectionManager();
            ICycleBellManager cbm = null; 

            try {
                cbm = new CycleBellManager("some_file_name", pcm, _mockTimerManager.Object);
            }
            catch (ArgumentNullException) { }

            var mainViewModel = cbm != null ? new MainViewModel (_mockDialogRegistrator.Object, cbm) : throw new ArgumentNullException(nameof(cbm), @"Instance of ICycleBellManager is null");

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
