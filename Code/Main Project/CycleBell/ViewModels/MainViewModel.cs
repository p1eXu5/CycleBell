﻿using System;
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
using CycleBellLibrary.Models;
using CycleBellLibrary.Timer;
using Microsoft.Win32;

namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        #region Fields

        public static SoundPlayer DefaultSoundPlayer;

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;
        private PresetViewModel _prevSelectedPreset;

        private string _initialDirectory;

        private bool _isRingOnStartTime = true;
        private bool _isFocused;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager)
        {
            _dialogRegistrator = dialogRegistrator ?? throw new ArgumentNullException(nameof(dialogRegistrator));

            _manager = cycleBellManager ?? throw new ArgumentNullException(nameof(cycleBellManager));
            _manager.CantCreateNewPresetEvent += OnCantCreateNewPresetEventHandler;
            LoadTimerManager(_manager);

            RemoveSelectedPresetCommand = new ActionCommand(RemoveSelectedPreset, CanRemoveSelectedPreset);
            ExportPresetsCommand = new ActionCommand(ExportPresets, CanExportPresets);
            ClearPresetsCommand = new ActionCommand(ClearPresets, CanExportPresets);

            LoadPresetViewModelCollection(_manager);

            DefaultSoundPlayer = new SoundPlayer(@"Sounds/default.wav"); 
        }

        private void LoadTimerManager(ICycleBellManager manager)
        {
            _timerManager = manager.TimerManager;
            _timerManager.TimerStartEvent += UpdateIsRunningAndTimerStateProperties;
            _timerManager.TimerStopEvent += UpdateIsRunningAndTimerStateProperties;
        }

        private void LoadPresetViewModelCollection(ICycleBellManager manager)
        {
            PresetViewModelCollection =
                new ObservableCollection<PresetViewModel>(manager.PresetCollectionManager.Presets.Select(p => new PresetViewModel(p, this)));

            if (PresetViewModelCollection.Count > 0) {
                SelectedPreset = PresetViewModelCollection[0];
            }

            ((INotifyCollectionChanged) (_manager.PresetCollectionManager.Presets)).CollectionChanged += OnPresetCollectionChangedEventHandler;
        }

        #endregion Constructor

        #region Properties

        // Preset
        public ObservableCollection<PresetViewModel> PresetViewModelCollection { get; set; }

        public PresetViewModel SelectedPreset
        {
            get => _selectedPreset;
            set {
                var newSelectedPreset = value;
                _prevSelectedPreset = _selectedPreset;

                if (_selectedPreset != null)
                    DisconnectHandlers(_selectedPreset);

                _selectedPreset = value;

                if (_selectedPreset != null)
                    ConnectHandlers(_selectedPreset);

                if (newSelectedPreset != null && _prevSelectedPreset != null && _prevSelectedPreset.Name == Preset.DefaultName)
                    _manager.CheckCreateNewPreset(_prevSelectedPreset.Preset);

                OnPropertyChanged();

                OnPropertyChanged(nameof(IsInfiniteLoop));
                OnPropertyChanged(nameof(HasNoName));
                OnPropertyChanged(nameof(IsSelectedPreset));
                OnPropertyChanged(nameof(IsNewPreset));
                ((ActionCommand)ExportPresetsCommand).RaiseCanExecuteChanged();
                ((ActionCommand)ClearPresetsCommand).RaiseCanExecuteChanged();
                ((ActionCommand)RemoveSelectedPresetCommand).RaiseCanExecuteChanged();
            }
        }

        public string SelectedPresetName
        {
            get => _selectedPreset?.Name;
            set {
                _manager.RenamePreset (_selectedPreset?.Preset, value);
                OnPropertyChanged ();
            }
        }
        
        public bool IsSelectedPreset => SelectedPreset != null;
        public bool IsNewPreset => SelectedPreset?.IsNewPreset ?? false;
        public bool IsInfiniteLoop
        {
            get => SelectedPreset?.IsInfiniteLoop ?? false;
            set {
                if (SelectedPreset != null) {

                    SelectedPreset.IsInfiniteLoop = value;
                    OnPropertyChanged ();
                }
            }
        }
        public bool IsPlayable => _selectedPreset?.TimePointVmCollection.Count > 0;

        // Timer
        public bool? TimerState
        {
            get {
                if (IsPaused) {

                    return null;
                }
                else if (IsRunning) {

                    return true;
                }
                else {
                    // Default
                    return false;
                }
            }
        }

        public bool IsRunning => _timerManager.IsRunning;

        public bool IsPaused => _timerManager.IsPaused;
        public bool IsStopped => IsRunning == false && IsPlayable;
            //set {
            //    if (!IsRunning) return;

            //    _timerManager.Stop();
            //    OnPropertyChanged();
            //}

        public string StartTimeName => _timerManager.StartTimeTimePointName;

        public bool IsRingOnStartTime 
        { 
            get => _isRingOnStartTime;
            set {
                _isRingOnStartTime = value;
                OnPropertyChanged ();
            }
        }

        public bool IsFocused
        {
            get => _isFocused;
            set {
                _isFocused = value;
                OnPropertyChanged ();
            }
        }

        public bool HasNoName
        {
            get {

                if (SelectedPreset != null) {
                    return String.IsNullOrWhiteSpace(SelectedPreset?.Name);
                }

                return false;
            }
        } 

        #endregion

        #region Commands

        // Menu File
        public ICommand CreateNewPresetCommand => new ActionCommand(CreateNewPreset);

        public ICommand AppendPresetsCommand => new ActionCommand(AppendPresets, CanAppendPresets);
        public ICommand ExportPresetsCommand { get; }

        public ICommand ClearPresetsCommand { get; }

        public ICommand ExitCommand => new ActionCommand(Exit);

        // Menu Settings and some timer buttons
        public ICommand RingOnStartTimeSwitchCommand => new ActionCommand (SwitchIsRingOnStartTime);
        public ICommand InfiniteLoopCommand => new ActionCommand((o) => { IsInfiniteLoop ^= true; });

        // Menu Help
        public ICommand ViewHelpCommand => new ActionCommand(About, o => false);
        public ICommand AboutCommand => new ActionCommand(About);

        // Presets ComboBox
        public ICommand PresetComboBoxReturnCommand => new ActionCommand(PresetComboBoxReturnKeyHandler);
        public ICommand RemoveSelectedPresetCommand { get; }
        public ICommand PresetLostFocusCommand => new ActionCommand(PresetLostFocus);

        // Timer buttons
        public ICommand MediaTerminalCommand => new ActionCommand(MediaTerminal);
        public ICommand StopCommand => new ActionCommand(Stop);
        public ICommand RingCommand => new ActionCommand(Ring);

        // MainWindow Events
        public ICommand OnClosingWindowCommand => new ActionCommand(OnClosingWindow);

        #endregion Commands

        #region Methods

        // PresetViewModelCollection changed handler
        /// <summary>
        /// Refreshes SelectedPreset when a new preset was added.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void OnPresetCollectionChangedEventHandler(object s, NotifyCollectionChangedEventArgs e)
        {
            if (e?.NewItems?[0] != null) { 

                PresetViewModelCollection.Add(new PresetViewModel((Preset)e.NewItems[0], this));
                SelectedPreset = PresetViewModelCollection[PresetViewModelCollection.Count - 1];
            }
            else if (e?.OldItems?[0] != null) {

                var deletingPresetVm = PresetViewModelCollection.First(pvm => pvm.Preset.Equals((Preset)e.OldItems[0]));

                var index = PresetViewModelCollection.IndexOf(_selectedPreset);
                PresetViewModelCollection.Remove(deletingPresetVm);
                if (index > 0) {
                    SelectedPreset = PresetViewModelCollection[index - 1];
                }
                else {
                    SelectedPreset = PresetViewModelCollection.Count > 0 ? PresetViewModelCollection[0] : null;
                }
            }
            else if (e != null && e.OldItems == null && e.NewItems == null) {

                PresetViewModelCollection.Clear();
                _prevSelectedPreset = null;
            }
        }
        
        // OnCan'tCreateNewPreset event handler
        private void OnCantCreateNewPresetEventHandler(object sender, CantCreateNewPreetEventArgs args)
        {
            if (args.CantCreateNewPresetReasonFlags == CantCreateNewPresetReasonsFlags.EmptyPresetNotModified) {

                _manager.DeletePreset(args.Preset);
                return;
            }

            var saveViewModel = new SavePresetDialogViewModel();

            if (_dialogRegistrator.ShowDialog(saveViewModel) == true) {

                var presetViewModel = _prevSelectedPreset ?? _selectedPreset;

                var renameViewModel = new RenamePresetDialogViewModel(presetViewModel);

                _dialogRegistrator.ShowDialog(renameViewModel);
                presetViewModel.Name = presetViewModel.Name;
            }
            else {
                _manager.DeletePreset(args.Preset);
            }
        }

        // timer handler
        private void UpdateIsRunningAndTimerStateProperties(object s, EventArgs e)
        {
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(TimerState));
        }

        private void RiseIsPlayableChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged (nameof(IsPlayable));
            OnPropertyChanged (nameof(IsStopped));
        }

        // add/remove SelectedPreset handlers
        /// <summary>
        /// Connect handlers to SelectedPreset
        /// </summary>
        /// <param name="selectedPresetViewModel"></param>
        private void ConnectHandlers(PresetViewModel selectedPresetViewModel)
        {
            selectedPresetViewModel.PropertyChanged += OnIsNewPresetChangedHandler;
            ((INotifyCollectionChanged)selectedPresetViewModel.TimePointVmCollection).CollectionChanged += RiseIsPlayableChanged;

            _timerManager.ChangeTimePointEvent += selectedPresetViewModel.OnTimePointChangedEventHandler;
            _timerManager.TimerSecondPassedEvent += selectedPresetViewModel.OnSecondPassedEventHandler;
            _timerManager.TimerPauseEvent += selectedPresetViewModel.OnTimerPauseEventHandler;
            _timerManager.TimerStopEvent += selectedPresetViewModel.OnTimerStopEventHandler;
        }

        /// <summary>
        /// Disconnect handlers from SelectedPreset
        /// </summary>
        private void DisconnectHandlers(PresetViewModel selectedPresetViewModel)
        {
            _timerManager.TimerStopEvent -= selectedPresetViewModel.OnTimerStopEventHandler;
            _timerManager.TimerPauseEvent -= selectedPresetViewModel.OnTimerPauseEventHandler;
            _timerManager.TimerSecondPassedEvent -= selectedPresetViewModel.OnSecondPassedEventHandler;
            _timerManager.ChangeTimePointEvent -= selectedPresetViewModel.OnTimePointChangedEventHandler;

            ((INotifyCollectionChanged)selectedPresetViewModel.TimePointVmCollection).CollectionChanged -= RiseIsPlayableChanged;
            selectedPresetViewModel.PropertyChanged -= OnIsNewPresetChangedHandler;
        }

        private void OnIsNewPresetChangedHandler(object s, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == "IsNewPreset") {

                OnPropertyChanged(nameof(IsNewPreset));
            }
        }

        //  File
        private void CreateNewPreset(object obj)
        {
            _manager.CreateNewPreset();
        }

        private void AppendPresets(object obj)
        {
            var fileName = GetPresetsFile();

            if (fileName == null)
                return;

            _manager.OpenPresets(fileName);
        }
        private bool CanAppendPresets(object obj)
        {
            return true;
        }

        private string GetPresetsFile()
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = _initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "xml files (*.xml)|*.xml",
            };

            if (ofd.ShowDialog() != true)
                return null;

            var fileName = ofd.FileName;
            _initialDirectory = Path.GetDirectoryName(fileName);

            return fileName;
        }

        private void ExportPresets(object obj)
        {
            var sfd = new SaveFileDialog()
                {
                    Filter = "xml file (*.xml)|*.xml",
                    InitialDirectory = _initialDirectory ?? Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
                };

            if (sfd.ShowDialog() != true)
                return;

            var fileName = sfd.FileName;
            _initialDirectory = Path.GetDirectoryName (fileName);

            _manager.SavePresets(fileName);
        }
        private bool CanExportPresets(object obj)
        {
            return PresetViewModelCollection.Count > 0;
        }

        private void ClearPresets(object obj)
        {
            _manager.ClearPresets();
        }

        private void Exit(object obj)
        {
            //_manager.SavePresets();
            System.Windows.Application.Current.Shutdown();
        }

        // Remove SelectedPreset
        private void RemoveSelectedPreset(object o)
        {
            _manager.DeletePreset(SelectedPreset?.Preset);
        }
        private bool CanRemoveSelectedPreset(object o)
        {
            return IsSelectedPreset;
        }

        // Preset ComboBox
        private void PresetComboBoxReturnKeyHandler (object newName)
        {
            //SelectedPreset.Name = newName.ToString();
            OnPropertyChanged(nameof(HasNoName));

            // Change value for call PropertyChangedCallback in attached property
            IsFocused ^= true;

            if (_selectedPreset != null)
                _selectedPreset.FocusStartTime = true;
        }
        private void PresetLostFocus(object obj) => OnPropertyChanged(nameof(HasNoName));

        // MediaTerminal - timer launcher
        private void MediaTerminal (object o)
        {
            var state = TimerState;

            if (state == true || state == null) {

                if (state == true) {
                    // if playing
                    _timerManager.Pause();
                }
                else {
                    // if paused
                    _timerManager.Resume();
                }

                OnPropertyChanged(nameof(IsPaused));
            }
            else {
                // if not running
                if (!_selectedPreset.Preset.TimerLoops.Values.Any(tl => tl <= 0)) {

                    _timerManager.PlayAsync(_selectedPreset.Preset);
                }
            }

            OnPropertyChanged(nameof(TimerState));
        }

        // ---- Stop
        private void Stop (object o)
        {
            _timerManager.Stop();
        }

        // ---- SwitchIsRingOnStartTime
        private void SwitchIsRingOnStartTime (object o)
        {
            DefaultSoundPlayer.Stop();
            IsRingOnStartTime ^= true;
        }

        // RingCommand
        private void Ring (object o)
        {
            IsRingOnStartTime = IsRingOnStartTime;
            Ring();
        }

        public void Ring(bool stopping = false)
        {
            DefaultSoundPlayer.Stop();

            if (!stopping) {
                DefaultSoundPlayer.Play();
            }
        }

        //  About
        private void About(object obj)
        {
            var viewModel = new AboutDialogViewModel();
            _dialogRegistrator.ShowDialog(viewModel);
        }

        // Closing MainWindow handler
        private void OnClosingWindow(object o)
        {
            if (_selectedPreset != null && _selectedPreset.Preset.PresetName == Preset.DefaultName) {
                _manager.CheckCreateNewPreset(_selectedPreset.Preset);
            }
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

