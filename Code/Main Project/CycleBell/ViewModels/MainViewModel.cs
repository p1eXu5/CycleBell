using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CycleBellLibrary;

using CycleBell.Base;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region Private

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private readonly ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager)
        {
            _dialogRegistrator = dialogRegistrator;
            _manager = cycleBellManager;
            _timerManager = cycleBellManager.TimerManager;

            var presetManager = cycleBellManager.PresetsManager;

            Presets = new ObservableCollection<PresetViewModel>(presetManager.Presets.Select(p => new PresetViewModel(p, _manager)));
            ((INotifyCollectionChanged)(presetManager.Presets)).CollectionChanged += (s, e) =>
                                                {
                                                    // TODO:
                                                    if (e.NewItems[0] != null)
                                                        Presets.Add( new PresetViewModel((Preset)e.NewItems[0], _manager) );
                                                };

        }

        #endregion Constructor

        #region CLR Properties

        public ObservableCollection<PresetViewModel> Presets { get; set; }

        public PresetViewModel SelectedPreset
        {
            get => _selectedPreset;
            set {
                SavePresetAsCommand.Execute (null);

                _selectedPreset = value;
                OnPropertyChanged ();
            }
        }

        public string Name
        {
            get => _selectedPreset?.Name;
            set {
                if (value != "")
                    _manager.RenamePreset (_selectedPreset?.Preset, value);

                OnPropertyChanged ();
            }
        }

        #endregion CLR Properties

        #region Commands

        // Done
        public ICommand CreateNewPresetCommand => new ActionCommand(CreateNewPreset);
        public ICommand ExitCommand => new ActionCommand(Exit);
        public ICommand AboutCommand => new ActionCommand(About);
        public ICommand SavePresetAsCommand => new ActionCommand(SavePresetAs, CanSavePresetAs);

        // In process
        public ICommand NewCommand => new ActionCommand(NewPresets);
        public ICommand OpenCommand => new ActionCommand(OpenPresets);
        public ICommand ExportPresetsCommand => new ActionCommand(SavePresets);
        public ICommand ViewHelpCommand => new ActionCommand(About);

        #endregion Commands

        #region Methods

        // CreateNewPresetCommand
        private void CreateNewPreset (object obj)
        {
            try {
                _manager.CreateNewPreset();
            }
            catch (InvalidOperationException) {

                SavePresetAsCommand (null);
            }
        }

        // SavePresetAsCommand
        private void SavePresetAs(object obj)
        {
            var viewModel = new SavePresetAsViewModel();
            bool? res = _dialogRegistrator.ShowDialog(viewModel);

            if (res == null || res == false) {
                _manager.DeletePreset (_selectedPreset.Preset);
            }
        }
        private bool CanSavePresetAs(object obj) => _selectedPreset?.IsModified ?? false;

        private void NewPresets(object obj)
        {
            // TODO:
        }

        private void OpenPresets(object obj)
        {
            // TODO:
        }

        private void SavePresets(object obj)
        {
            // TODO:
        }

        private void Exit(object obj) => System.Windows.Application.Current.Shutdown();

        // AboutCommand
        private void About(object obj)
        {
            var viewModel = new AboutDialogViewModel();
            _dialogRegistrator.ShowDialog(viewModel);
        }

        #endregion Methods

    }

    #region Converters

    public class CycleBellStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CycleBellStateFlags)) {
                return null;
            }

            var stateFlags = (CycleBellStateFlags) value;

            if ((stateFlags & CycleBellStateFlags.FirstCall) == CycleBellStateFlags.FirstCall) {
                return "FirstCall";
            }
            else if ((stateFlags & CycleBellStateFlags.InfiniteLoop) == CycleBellStateFlags.InfiniteLoop) {
                return "InfiniteLoop";
            }

            return "Unstate";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Menu icon converter
    /// </summary>
    public class FlagToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CycleBellStateFlags)) {
                return null;
            }

            var stateFlags = (CycleBellStateFlags) value;

            if ((byte)stateFlags == 0) {
                return 0.0;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion Converters
}
