using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Data;
using System.Windows.Input;

using CycleBell.Base;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Microsoft.Win32;

namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        #region Private
        public static SoundPlayer DefaultSoundPlayer = new SoundPlayer(); 

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private readonly ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;
        private string _initialDirectory = null;

        private bool _isNewPreset;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager)
        {
            _dialogRegistrator = dialogRegistrator ?? throw new ArgumentNullException(nameof(dialogRegistrator));

            _manager = cycleBellManager ?? throw new ArgumentNullException(nameof(cycleBellManager));
            _manager.CantCreateNewPresetEvent += () => SavePresetAs(null);

            var presetManager = cycleBellManager.PresetCollectionManager;
            Presets = new ObservableCollection<PresetViewModel>(presetManager.Presets.Select(p => new PresetViewModel(p, this)));

            ((INotifyCollectionChanged) (presetManager.Presets)).CollectionChanged += PresetCollectionEventHandler;

            _timerManager = cycleBellManager.TimerManager;

            AppDomain.CurrentDomain.ProcessExit += (s, e) => SavePresetsBeforeExit (null);
        }

        #endregion Constructor

        #region Properties

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
        public bool IsSelectedPreset => SelectedPreset != null;
        public bool IsNewPreset
        {
            get => SelectedPreset?.IsNewPreset ?? false;
        }

        public bool IsRunning => _timerManager.IsRunning;

        public bool IsInfiniteLoop
        {
            get => SelectedPreset?.IsInfiniteLoop == true;
            set {
                if (SelectedPreset != null) {

                    SelectedPreset.IsInfiniteLoop = value;
                    OnPropertyChanged ();
                }
            }
        }

        #endregion CLR Properties

        #region Commands

        // Menu
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

        // Media
        public ICommand PlayCommand => new ActionCommand (Play, CanPlay);
        public ICommand StopCommand => new ActionCommand (Stop, CanStop);
        public ICommand PauseCommand => new ActionCommand (Pause, CanPause);
        public ICommand ResumeCommand => new ActionCommand (Resume, CanResume);
        public ICommand RingCommand => new ActionCommand(Ring);

        #endregion Commands

        #region Methods

        private void PresetCollectionEventHandler(object s, NotifyCollectionChangedEventArgs e)
        {
            if (e?.NewItems?[0] != null && e.OldItems?[0] == null) {

                Presets.Add(new PresetViewModel((Preset)e.NewItems[0], this));
                SelectedPreset = Presets[Presets.Count - 1];
                SelectedPreset.PropertyChanged += IsNewPresetChangedHandler;
            }

            if (e?.OldItems?[0] != null && e?.NewItems?[0] == null) {

                var deletingPresetVm = Presets.First(pvm => pvm.Preset.Equals((Preset)e.OldItems[0]));

                if (deletingPresetVm == SelectedPreset)
                    SelectedPreset.PropertyChanged -= IsNewPresetChangedHandler;

                Presets.Remove(deletingPresetVm);
                SelectedPreset = Presets.Count > 0 ? Presets[0] : null;
            }

            OnPropertyChanged(nameof(SelectedPreset));
            OnPropertyChanged(nameof(IsNewPreset));
        }

        private void IsNewPresetChangedHandler(object s, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == "IsNewPreset") {

                OnPropertyChanged(nameof(IsNewPreset));
            }
        }

        //  Save Preset
        private void CreateNewPreset (object obj)
        {
            _manager.CreateNewPreset();
            OnPropertyChanged(nameof(IsSelectedPreset));
        }

        //  Save Preset
        private void SavePreset(object obj)
        {
            SelectedPreset.Save();
        }
        private bool CanSavePreset(object obj)
        {
            return SelectedPreset?.IsModified == true;
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

        //  Import/Export Presets
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

        //  Save presets before exit
        private void SavePresetsBeforeExit(object obj)
        {
             _manager.SavePresets();
        }

        //  Exit
        private void Exit(object obj)
        {
            //_manager.SavePresets();
            System.Windows.Application.Current.Shutdown();
        }

        //  About
        private void About(object obj)
        {
            var viewModel = new AboutDialogViewModel();
            _dialogRegistrator.ShowDialog(viewModel);
        }

        // ---- Play
        private void Play (object o)
        {
            _timerManager.PlayAsync (SelectedPreset.Preset);
        }
        private bool CanPlay (object o)
        {
            return (SelectedPreset?.TimePointVmCollection.Count > 0)
                && !_timerManager.IsRunning;
        }

        // ---- Stop
        private void Stop (object o)
        {
            _timerManager.Stop();
        }
        private bool CanStop (object o)
        {
            return _timerManager.IsRunning;
        }
        
        // ---- Pause
        private void Pause (object o)
        {
            _timerManager.Pause();
        }
        private bool CanPause (object o)
        {
            return _timerManager.IsRunning;
        }
        
        // ---- Resume
        private void Resume (object o)
        {
            _timerManager.Resume();
        }
        private bool CanResume (object o)
        {
            return _timerManager.IsPaused;
        }

        // RingCommand
        private void Ring (object o) => DefaultSoundPlayer.Play();



        #endregion Methods

        #region IMainViewModel impl

        public IDictionary<int, SoundPlayer> SoundMap { get; } = new Dictionary<int, SoundPlayer>();
        public void Ring (int id) { throw new NotImplementedException(); }

        #endregion
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
