/*
 * Copyright © 2018 Vladimir Likhatskiy. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
        private const int _DUE_TIME = 5000;

        #region Fields

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;

        private string _initialDirectory;

        private bool _isRingOnStartTime = true;
        private bool _moveFocusRightTrigger;

        private string _statusBarText = "";

        /// <summary>
        /// For clean up status bar.
        /// </summary>
        private readonly Timer _timer;

        private bool _dontSwitchSelectedPreset;
        private bool _checked;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager, IAlarm alarm )
        {
            _dialogRegistrator = dialogRegistrator ?? throw new ArgumentNullException(nameof(dialogRegistrator));
            Alarm = alarm ?? throw new ArgumentNullException(nameof(alarm), @"Alarm cannot be null.");

            _manager = cycleBellManager ?? throw new ArgumentNullException(nameof(cycleBellManager));
            _manager.CantCreateNewPresetEvent += OnCantCreateNewPresetEventHandler;
            LoadTimerManager(_manager);

            InitializeCommands();

            LoadPresetViewModelCollection(_manager);

            if ( File.Exists( @"Sounds/default.wav" ) ) {
                var path = Path.GetDirectoryName( System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName ) + "/Sounds/default.wav";
                Alarm.SetDefaultSound( path );
            }

            _timer = new Timer(ClearStatusBarText, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void InitializeCommands()
        {
            RemoveSelectedPresetCommand = new ActionCommand(RemoveSelectedPreset, CanRemoveSelectedPreset);
            ExportPresetsCommand = new ActionCommand(ExportPresets, CanExportPresets);
            ClearPresetsCommand = new ActionCommand(ClearPresets, CanExportPresets);
        }

        private void LoadTimerManager(ICycleBellManager manager)
        {
            _timerManager = manager.TimerManager;
            _timerManager.TimerStarted += UpdateIsRunningAndTimerStateProperties;
            _timerManager.TimerStopEvent += UpdateIsRunningAndTimerStateProperties;
        }

        private void LoadPresetViewModelCollection(ICycleBellManager manager)
        {
            if (manager.PresetCollectionManager.Presets.Any(manager.IsNewPreset)) {
                throw new ArgumentException("ICycleBellManager.IPresetCollectionManager.Presets contain NewPreset");
            }

            PresetViewModelCollection =
                new ObservableCollection<PresetViewModel>(manager.PresetCollectionManager.Presets.Where(p => p.IsNotNew()).Select(p => new PresetViewModel(p, this)));

            if (PresetViewModelCollection.Count > 0) {
                SelectedPreset = PresetViewModelCollection[0];
            }

            ((INotifyCollectionChanged) (_manager.PresetCollectionManager.Presets)).CollectionChanged += OnPresetCollectionChangedEventHandler;
        }

        #endregion Constructor

        #region Properties

        // Preset
        public ObservableCollection<PresetViewModel> PresetViewModelCollection { get; set; }

        public IAlarm Alarm { get; }

        public PresetViewModel SelectedPreset
        {
            get => _selectedPreset;
            set {
                if (UpdateSelectedPreset(value)) {
                    OnSelectedPresetPropertyChanged();
                }
            }
        }

        public string SelectedPresetName
        {
            get => _selectedPreset?.Name;
            set {
                // Because when preset has deleted we are don't know what
                // preset will be next.
                _manager.RenamePreset(_selectedPreset?.Preset, value);

                OnPropertyChanged ();
            }
        }
        
        public bool IsSelectedPreset => SelectedPreset != null;
        public bool IsNewPreset => SelectedPreset?.IsNew ?? false;
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
                if ( IsPaused ) { return null; }
                if ( IsRunning ) { return true; }
                return false;
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

        public string StartTimeName => _timerManager.StartTimePointName;

        public bool IsRingOnStartTime 
        { 
            get => _isRingOnStartTime;
            set {
                _isRingOnStartTime = value;
                OnPropertyChanged ();
            }
        }

        public bool MoveFocusRightTrigger
        {
            get => _moveFocusRightTrigger;
            set {
                _moveFocusRightTrigger = value;
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

        public string StatusBarText
        {
            get => _statusBarText;
            set {
                _statusBarText = value;
                OnPropertyChanged();

                if (!String.IsNullOrWhiteSpace(_statusBarText)) {
                    RunStatusBarTimer();
                }
            }
        }

        #endregion

        #region Commands

        // Menu File
        public ICommand CreateNewPresetCommand => new ActionCommand(CreateNewPreset);

        public ICommand AppendPresetsCommand => new ActionCommand(AppendPresets, CanAppendPresets);
        // Is initialized in ctor
        public ICommand ExportPresetsCommand { get; set; }

        // Is initialized in ctor
        public ICommand ClearPresetsCommand { get; set; }

        public ICommand ExitCommand => new ActionCommand(Exit);

        // Menu Settings and some timer buttons
        public ICommand RingOnStartTimeSwitchCommand => new ActionCommand( SwitchIsRingOnStartTime );
        public ICommand InfiniteLoopCommand => new ActionCommand((o) => { IsInfiniteLoop ^= true; });

        // Menu Help
        public ICommand ViewHelpCommand => new ActionCommand(About, o => false);
        public ICommand AboutCommand => new ActionCommand(About);

        // Presets ComboBox
        public ICommand PresetComboBoxReturnCommand => new ActionCommand( PresetComboBoxReturnKeyHandler );
        // Is initialized in ctor
        public ICommand RemoveSelectedPresetCommand { get; set; }
        public ICommand PresetLostFocusCommand => new ActionCommand( PresetLostFocus );

        // Timer buttons
        public ICommand MediaTerminalCommand => new ActionCommand( MediaTerminal );
        public ICommand StopCommand => new ActionCommand( Stop );
        public ICommand RingCommand => new ActionCommand( Ring );
        public ICommand ChangeDefaultSoundCommand => new ActionCommand( ChangeDefaultSound );

        // MainWindow Events
        public ICommand OnClosingWindowCommand => new ActionCommand( OnClosingWindow );

        #endregion Commands

        #region Methods

        /// <summary>
        /// Shows SavePresetDialog, then if true RenamePresetDialog.
        /// </summary>
        /// <returns>false - if user cansel save preset, true - if preset renamed</returns>
        private bool ShowSavePresetDialog (PresetViewModel preset)
        {
            var saveViewModel = new SavePresetDialogViewModel();

            if (_dialogRegistrator.ShowDialog (saveViewModel) != true) 
                return false;

            var renamePresetDialogViewModel = new RenamePresetDialogViewModel(preset);

            _dialogRegistrator.ShowDialog (renamePresetDialogViewModel);

            return true;
        }

        /// <summary>
        /// Refreshes SelectedPreset when a new preset was added.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void OnPresetCollectionChangedEventHandler(object s, NotifyCollectionChangedEventArgs e)
        {
            // add
            if (e?.NewItems?[0] != null) { 

                PresetViewModelCollection.Add(new PresetViewModel((Preset)e.NewItems[0], this));

                if (!_dontSwitchSelectedPreset && (_selectedPreset == null || !_selectedPreset.IsNew)) {

                    // При добавлении пресета в коллекцию SelectedPreset не изменяется
                    SelectedPreset = PresetViewModelCollection[PresetViewModelCollection.Count - 1];
                }
            }
            // remove
            else if (e?.OldItems?[0] != null) {

                var deletingPresetVm = PresetViewModelCollection.First(pvm => pvm.Preset.Equals((Preset)e.OldItems[0]));

                // Collection switches selected preset to null
                PresetViewModelCollection.Remove(deletingPresetVm);

                if (_selectedPreset == null && PresetViewModelCollection.Count > 0) {

                    SelectedPreset = PresetViewModelCollection[0];
                }
            }
            // clear
            else if (e != null && e.OldItems == null && e.NewItems == null) {

                PresetViewModelCollection.Clear();
            }

            MoveFocusRightFromPresetComboBox();
        }

        /// <summary>
        /// Move ui element's focus to the right  
        /// </summary>
        private void MoveFocusRightFromPresetComboBox()
        {
            MoveFocusRightTrigger ^= true;
        }

        // OnCan'tCreateNewPreset event handler
        private void OnCantCreateNewPresetEventHandler(object sender, CantCreateNewPreetEventArgs args)
        {
            if (_selectedPreset?.IsNew == true) {

                StatusBarText = "The new preset.";
                return;
            }

            SelectedPreset = GetPresetViewModelWithNewPreset();
            StatusBarText = "Switched to the new preset.";
        }

        private PresetViewModel GetPresetViewModelWithNewPreset()
        {
            PresetViewModel presetVm = PresetViewModelCollection.FirstOrDefault(p => p.IsNew);
            return presetVm;
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

        private void RunStatusBarTimer()
        {
            _timer.Change(_DUE_TIME, Timeout.Infinite);
        }

        private void ClearStatusBarText (object state)
        {
            StatusBarText = "";
        }

        /// <summary>
        /// Updates properties and commands after SelectedPreset change.
        /// </summary>
        private void OnSelectedPresetPropertyChanged()
        {
            OnPropertyChanged(nameof(SelectedPreset));
            OnPropertyChanged(nameof(SelectedPresetName));
            OnPropertyChanged(nameof(IsInfiniteLoop));
            OnPropertyChanged(nameof(HasNoName));
            OnPropertyChanged(nameof(IsSelectedPreset));
            OnPropertyChanged(nameof(IsNewPreset));
            ((ActionCommand)ExportPresetsCommand).RaiseCanExecuteChanged();
            ((ActionCommand)ClearPresetsCommand).RaiseCanExecuteChanged();
            ((ActionCommand)RemoveSelectedPresetCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Deletes Timer handlers from the oldValue of SelectedPreset
        /// than connects to the newValue of SelectedPreset.
        /// </summary>
        /// <param name="newSelectedPreset"></param>
        private bool UpdateSelectedPreset (PresetViewModel newSelectedPreset)
        {
            if (newSelectedPreset == null) {

                _selectedPreset = null;
                return true;
            }

            var currentPreset = _selectedPreset;

            if (ReferenceEquals(currentPreset, newSelectedPreset)) {
                return false;
            }

            if (currentPreset != null) {
                DisconnectHandlers(currentPreset);
            }

            if (currentPreset != null && currentPreset.IsNew && currentPreset.IsModified && !ShowSavePresetDialog(currentPreset)) {
                // The deleting preset will be deleted. After that
                // the selected preset will be try to set himself again
                // and UpdateSelectedPreset will be invoked again.
                _selectedPreset = null;
                _manager.RemovePreset (currentPreset.Preset);
            }

            _selectedPreset = newSelectedPreset;
            ConnectHandlers (_selectedPreset);

            return true;
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

            _timerManager.TimePointChanged += selectedPresetViewModel.OnTimePointChangedEventHandler;
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
            _timerManager.TimePointChanged -= selectedPresetViewModel.OnTimePointChangedEventHandler;

            ((INotifyCollectionChanged)selectedPresetViewModel.TimePointVmCollection).CollectionChanged -= RiseIsPlayableChanged;
            selectedPresetViewModel.PropertyChanged -= OnIsNewPresetChangedHandler;
        }

        private void OnIsNewPresetChangedHandler(object s, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == "IsNew") {

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

            if (SelectedPreset != null) {
                _dontSwitchSelectedPreset = true;
            }

            _manager.OpenPresets(fileName);

            if (_dontSwitchSelectedPreset) {
                _dontSwitchSelectedPreset = false;
            }
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
            CheckSelectedPresetBeforeExit();
            System.Windows.Application.Current.Shutdown();
            // -> OnClosingWindow(object o);
        }

        // Remove SelectedPreset
        private void RemoveSelectedPreset(object o)
        {
            _manager.RemovePreset(SelectedPreset?.Preset);
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

            // Change newSelectedPreset for call PropertyChangedCallback in attached property
            MoveFocusRightTrigger ^= true;

            if (_selectedPreset != null)
                _selectedPreset.FocusStartTime = true;
        }
        private void PresetLostFocus(object obj) => OnPropertyChanged(nameof(HasNoName));

        // Sound
        public void Ring(bool stopping = false)
        {
            Alarm.StopDefault();

            if (!stopping) {
                Alarm.PlayDefault();
            }
        }

        private void Ring (object o)
        {
            IsRingOnStartTime = IsRingOnStartTime;
            Ring();
        }
        private void Stop (object o)
        {
            Alarm.StopDefault();
            _timerManager.Stop();
        }

        private void MediaTerminal (object o)
        {
            var state = TimerState;

            if (state == true || state == null) {

                if (state == true) {
                    // if playing
                    Alarm.StopDefault();
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
                if ( !_selectedPreset.Preset.TimerLoops.Values.Any( tl => tl <= 0 ) ) {

                    _timerManager.PlayAsync( _selectedPreset.Preset );
                }
            }

            OnPropertyChanged(nameof(TimerState));
        }
        
        private void SwitchIsRingOnStartTime (object o)
        {
            Alarm.Stop();
            Alarm.StopDefault();
            IsRingOnStartTime ^= true;
        }

        private void ChangeDefaultSound ( object o )
        {
            var ofd = new OpenFileDialog {
                Filter = "mp3, wav|*.mp3;*.wav|all files|*.*",
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyMusic ),
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true
            };
            if ( ofd.ShowDialog() == true ) {
                Alarm.SetDefaultSound( ofd.FileName );
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
            CheckSelectedPresetBeforeExit();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckSelectedPresetBeforeExit()
        {
            if (_checked) {
                return;
            }

            if (_selectedPreset?.IsNew == true) {

                if (_selectedPreset.IsModified) {

                    if (!ShowSavePresetDialog(_selectedPreset)) {
                        _manager.RemovePreset(_selectedPreset.Preset);
                    }
                }
                else {
                    _manager.RemovePreset(_selectedPreset.Preset);
                }
            }

            _checked = true;
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

