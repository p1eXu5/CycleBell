using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CycleBellLibrary;

using CycleBell.Base;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Microsoft.Win32;

namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region Private

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private readonly ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;
        private string _initialDirectory = null;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager)
        {
            _dialogRegistrator = dialogRegistrator ?? throw new ArgumentNullException(nameof(dialogRegistrator));

            _manager = cycleBellManager ?? throw new ArgumentNullException(nameof(cycleBellManager));

            _timerManager = cycleBellManager.TimerManager;

            var presetManager = cycleBellManager.PresetsManager;

            Presets = new ObservableCollection<PresetViewModel>(presetManager.Presets.Select(p => new PresetViewModel(p, _manager)));

            ((INotifyCollectionChanged)(presetManager.Presets)).CollectionChanged += (s, e) =>
                                                {
                                                    if (e?.NewItems?[0] != null && e.OldItems?[0] == null)
                                                        Presets.Add( new PresetViewModel((Preset)e.NewItems[0], _manager) );

                                                    if (e?.OldItems?[0] != null && e?.NewItems?[0] == null)
                                                    {
                                                        var deletingPresetVm = Presets.First (pvm => pvm.Preset.Equals ((Preset) e.OldItems[0]));
                                                        Presets.Remove (deletingPresetVm);
                                                    }
                                                };

            AppDomain.CurrentDomain.ProcessExit += (s, e) => SavePresetsBeforeExitCommand.Execute (null);

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

        public ICommand SavePresetCommand => new ActionCommand(SavePreset, CanSavePreset);
        public ICommand SavePresetAsCommand => new ActionCommand(SavePresetAs, CanSavePresetAs);

        public ICommand ImportPresetsCommand => new ActionCommand(ImportPresets);
        public ICommand ExportPresetsCommand => new ActionCommand(ExportPresets, CanExportPresets);

        public ICommand ExitCommand => new ActionCommand(Exit);

        public ICommand AboutCommand => new ActionCommand(About);

        public ICommand SavePresetsBeforeExitCommand => new ActionCommand(SavePresetsBeforeExit);

        // In process
        public ICommand ViewHelpCommand => new ActionCommand(About);

        #endregion Commands

        #region Methods

        // ---- Save Preset
        private void CreateNewPreset (object obj)
        {
            try {
                _manager.CreateNewPreset();
            }
            catch (InvalidOperationException) {

                SavePresetAsCommand.Execute (null);
            }
        }

        // ---- Save Preset
        private void SavePreset(object obj)
        {
            SelectedPreset.Save();
        }
        private bool CanSavePreset(object obj)
        {
            return SelectedPreset?.CanSave() ?? false;
        }

        private void SavePresetAs(object obj)
        {
            if (_selectedPreset.Name == Preset.DefaultName) {

                var viewModel = new SavePresetAsViewModel();
                bool? res = _dialogRegistrator.ShowDialog (viewModel);

                if (res == null || res == false) {

                    _manager.DeletePreset (_selectedPreset.Preset);
                }
            }
        }
        private bool CanSavePresetAs(object obj) => _selectedPreset?.IsModified ?? false;

        // ---- Import/Export Presets
        private void ImportPresets(object obj)
        {
            var ofd = new OpenFileDialog
                {
                    InitialDirectory = _initialDirectory ?? System.Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
                    Filter = "xml files (*.xml)|*.xml",
                };

            if (ofd.ShowDialog() != true)
                return;

            var fileName = ofd.FileName;
            _initialDirectory = Path.GetDirectoryName (fileName);

            _manager.OpenPresets(fileName);
        }

        private void ExportPresets(object obj)
        {
            var sfd = new SaveFileDialog()
                {
                    Filter = "xml file (*.xml)|*.xml",
                    InitialDirectory = _initialDirectory ?? System.Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
                };

            if (sfd.ShowDialog() != true)
                return;

            var fileName = sfd.FileName;
            _initialDirectory = Path.GetDirectoryName (fileName);

            _manager.SavePresets(fileName);
        }
        private bool CanExportPresets(object obj)
        {
            return Presets.Count > 0;
        }

        // ---- Save presets before exit
        private void SavePresetsBeforeExit(object obj)
        {
             _manager.SavePresets();
        }

        // ---- Exit
        private void Exit(object obj)
        {
            //_manager.SavePresets();
            System.Windows.Application.Current.Shutdown();
        }

        // ---- About
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
