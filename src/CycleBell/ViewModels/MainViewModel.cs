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
using System.Collections.Generic;
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
using CycleBell.Engine;
using CycleBell.Engine.Models;
using CycleBell.Engine.Timer;
using Microsoft.Win32;


[assembly:InternalsVisibleTo("CycleBell.Tests")]
namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        #region consts

        private const int _DUE_TIME = 5000;
        internal const string REG_PATH = @"Software\CycleBell\Settings\";
        internal const string DEFAULT_SOUND_KEY = "Default Sound";
        internal const string SELECTED_PRESET_KEY = "Selected Preset";

        #endregion


        #region fields

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
        private bool _isPlayDefault;

        #endregion Private


        #region ctor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager, IAlarm alarm )
        {
            void InitializeCommands()
            {
                RemoveSelectedPresetCommand = new ActionCommand( RemoveSelectedPreset, CanRemoveSelectedPreset );
                ExportPresetsCommand = new ActionCommand( ExportPresets, CanExportPresets );
                ClearPresetsCommand = new ActionCommand( ClearPresets, CanExportPresets );
            }

            void LoadTimerManager()
            {
                _timerManager = _manager.TimerManager;
                _timerManager.TimerStarted += UpdateIsRunningAndTimerStateProperties;
                _timerManager.TimerStopped += UpdateIsRunningAndTimerStateProperties;
            }
            
            void LoadPresetViewModelCollection()
            {
                if (_manager.PresetCollection.Presets.Any(_manager.IsNewPreset)) {
                    throw new ArgumentException("ICycleBellManager.IOpenSave.Presets contain NewPreset");
                }

                PresetViewModelCollection =
                    new ObservableCollection<PresetViewModel>(_manager.PresetCollection.Presets.Where(p => p.IsNotNew()).Select(p => new PresetViewModel(p, this)));

                if (PresetViewModelCollection.Count > 0) {
                    SelectedPreset = PresetViewModelCollection[0];
                }

                ((INotifyCollectionChanged) (_manager.PresetCollection.Presets)).CollectionChanged += OnPresetCollectionChangedEventHandler;
            }

            void LoadDefaultSounds()
            {
                DefaultSoundVmCollection = new List< SoundMenuItemViewModel >( 
                    Alarm.DefaultSoundCollection
                         .Select( s => {
                             var ind1 = s.LocalPath.LastIndexOf( '\\' );
                             if ( ind1 == -1 ) ind1 = 0;
                             else ind1 += 1;

                             var name = s.LocalPath.Substring( ind1 );

                             var ind2 = name.LastIndexOf( '.' );
                             if ( ind2 > -1 ) {
                                 name = name.Substring( 0, ind2 );
                             }

                             return new SoundMenuItemViewModel {
                                 Name = name,
                                 Command = new ActionCommand( o => {
                                     Alarm.SetDefaultSound( s );
                                     OnMediaEnded( this, EventArgs.Empty );
                                 } )
                             };
                         } ) 
                );
            }


            _dialogRegistrator = dialogRegistrator ?? throw new ArgumentNullException(nameof(dialogRegistrator));
            Alarm = alarm ?? throw new ArgumentNullException(nameof(alarm), @"Alarm cannot be null.");

            _manager = cycleBellManager ?? throw new ArgumentNullException(nameof(cycleBellManager));
            _manager.CantCreateNewPresetEvent += OnCantCreateNewPresetEventHandler;
            
            LoadTimerManager();

            InitializeCommands();

            LoadPresetViewModelCollection();

            if ( Alarm.DefaultSoundCollection.Any() ) {
                LoadDefaultSounds();
            }

            _timer = new Timer(ClearStatusBarText, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion


        #region properties

        // Preset
        public ObservableCollection< PresetViewModel > PresetViewModelCollection { get; set; }

        public IEnumerable< SoundMenuItemViewModel > DefaultSoundVmCollection { get; private set; }

        public IAlarm Alarm { get; }

        public PresetViewModel SelectedPreset
        {
            get => _selectedPreset;
            set {
                if ( UpdateSelectedPreset( value ) ) {
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
        
        public bool IsSelectedPresetExist => SelectedPreset != null;

        public bool IsNewPreset => IsSelectedPresetExist && _manager.IsNewPreset( SelectedPreset.Preset );
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

        /// <summary>
        /// If timer stopped returns true, if paused - null, otherwise - false.
        /// </summary>
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


        #region commands

        // File menu

        #region LoadUserSettingsCommand

        public ICommand LoadUserSettingsCommand => new ActionCommand(LoadUserSettings, CanLoadUserSettings);

        private void LoadUserSettings(object o)
        {
            var key = Registry.CurrentUser.OpenSubKey( REG_PATH );

            // ReSharper disable once PossibleNullReferenceException
            var defaultSound = key.GetValue( DEFAULT_SOUND_KEY )?.ToString() ?? "";
            var savedSelectedPreset = key.GetValue( SELECTED_PRESET_KEY )?.ToString() ?? "";
            key.Close();

            try {
                Alarm.SetDefaultSound( new Uri( defaultSound ) );
            }
            catch ( UriFormatException ) { }

            var presetVm = PresetViewModelCollection.FirstOrDefault( p => p.Name.Equals( savedSelectedPreset ) );

            if ( presetVm != null ) {
                SelectedPreset = presetVm;
            }
        }

        private bool CanLoadUserSettings(object o)
        {
            var key = Registry.CurrentUser.OpenSubKey( REG_PATH );
            return PresetViewModelCollection.Any() && key != null;
        }

        #endregion


        #region SaveUserSettingsCommand

        public ICommand SaveUserSettingsCommand => new ActionCommand( _ => {
            try {
                SaveUserSettings();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch {

            }
        } );

        private void SaveUserSettings()
        {
            var key = Registry.CurrentUser.OpenSubKey( MainViewModel.REG_PATH, true );

            if ( key == null ) {
                key = Registry.CurrentUser.CreateSubKey( MainViewModel.REG_PATH );
            }

            
            key.SetValue( MainViewModel.SELECTED_PRESET_KEY, _selectedPreset?.Name ?? "" );
            var defaultSound = Alarm.GetDefaultSound()?.ToString() ?? "";

            key.SetValue( MainViewModel.DEFAULT_SOUND_KEY, defaultSound );

            key.Close();
        }

        #endregion


        #region CreateNewPresetCommand

        public ICommand CreateNewPresetCommand => new ActionCommand(CreateNewPreset);

        private void CreateNewPreset(object obj)
        {
            _manager.CreateNewPreset();
        }

        #endregion


        #region AppendPresetsCommand

        public ICommand AppendPresetsCommand => new ActionCommand(AppendPresets, CanAppendPresets);

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

            OnPropertyChanged( nameof( IsPlayable ) );
        }

        private bool CanAppendPresets(object obj)
        {
            return true;
        }

        #endregion


        #region ExportPresetsCommand

        public ICommand ExportPresetsCommand { get; set; }

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

        #endregion


        #region ClearPresetsCommand

        public ICommand ClearPresetsCommand { get; set; }
        private void ClearPresets(object obj)
        {
            _manager.ClearPresets();

            OnPropertyChanged( nameof( IsPlayable ) );
        }

        #endregion


        #region RemoveSelectedPresetCommand

        public ICommand RemoveSelectedPresetCommand { get; set; }

        private void RemoveSelectedPreset(object o)
        {
            _manager.RemovePreset(SelectedPreset?.Preset);
        }

        private bool CanRemoveSelectedPreset(object o)
        {
            return IsSelectedPresetExist;
        }

        #endregion


        // Settings menu

        #region RingOnStartTimeSwitchCommand

        public ICommand RingOnStartTimeSwitchCommand => new ActionCommand( SwitchIsRingOnStartTime );

        private void SwitchIsRingOnStartTime (object o)
        {
            IsRingOnStartTime ^= true;
        }

        #endregion


        #region InfiniteLoopCommand

        public ICommand InfiniteLoopCommand => new ActionCommand((o) => { IsInfiniteLoop ^= true; });

        #endregion


        // Help menu

        #region ViewHelpCommand

        public ICommand ViewHelpCommand => new ActionCommand(OpenGettingStarted);

        private void OpenGettingStarted(object o)
        {
            System.Diagnostics.Process.Start("https://www.cyclebell.com/getting_started");
        }

        #endregion


        #region AboutCommand

        public ICommand AboutCommand => new ActionCommand(About);

        private void About(object obj)
        {
            var viewModel = new AboutDialogViewModel();
            _dialogRegistrator.ShowDialog(viewModel);
        }

        #endregion


        // Focus commands

        #region PresetComboBoxReturnCommand

        public ICommand PresetComboBoxReturnCommand => new ActionCommand( PresetComboBoxReturnKeyHandler );

        private void PresetComboBoxReturnKeyHandler (object newName)
        {
            //SelectedPreset.Name = newName.ToString();
            OnPropertyChanged(nameof(HasNoName));

            // Change newSelectedPreset for call PropertyChangedCallback in attached property
            MoveFocusRightTrigger ^= true;

            if (_selectedPreset != null)
                _selectedPreset.FocusStartTime = true;
        }

        #endregion


        #region PresetLostFocusCommand

        public ICommand PresetLostFocusCommand => new ActionCommand( PresetLostFocus );

        private void PresetLostFocus(object obj) => OnPropertyChanged(nameof(HasNoName));

        #endregion


        // Media buttons

        #region MediaTerminalCommand

        // Timer buttons
        public ICommand MediaTerminalCommand => new ActionCommand(MediaTerminal);

        private void MediaTerminal(object o)
        {
            var state = TimerState;

            if (state == true || state == null)
            {

                if (state == true)
                {
                    // if playing
                    Alarm.StopDefault();
                    _timerManager.Pause();
                }
                else
                {
                    // if paused
                    _timerManager.Resume();
                }

                OnPropertyChanged(nameof(IsPaused));
            }
            else
            {
                // if not running
                if (!_selectedPreset.Preset.TimerLoopDictionary.Values.Any( tl => tl <= 0 ))
                {
                    Alarm.Reset();
                    _timerManager.PlayAsync( _selectedPreset.Preset );
                }
            }

            OnPropertyChanged(nameof(TimerState));
        }

        #endregion


        #region StopCommand

        public ICommand StopCommand => new ActionCommand(Stop);

        private void Stop(object o)
        {
            Alarm.Stop();
            _timerManager.Stop();
        }

        #endregion


        #region RingCommand

        public ICommand RingCommand => new ActionCommand(Ring);

        private void Ring (object o)
        {

            if ( !_isPlayDefault ) {
                Alarm.DefaultMediaEnded += OnMediaEnded;
                Alarm.PlayDefault();
                _isPlayDefault = true;
            }
            else {
                Alarm.StopDefault();
                Alarm.DefaultMediaEnded -= OnMediaEnded;
                _isPlayDefault = false;
            }

            IsRingOnStartTime = IsRingOnStartTime;
        }

        #endregion


        // Window handlers

        #region ExitCommand

        /// <summary>
        /// Menu command: File -> Exit
        /// </summary>
        public ICommand ExitCommand => new ActionCommand( Exit );

        private void Exit(object obj)
        {
            CheckSelectedPresetBeforeExit();
            System.Windows.Application.Current.Shutdown();
            // -> CloseWindow(object o);
        }

        #endregion


        #region CloseCommand

        /// <summary>
        /// MainWindow Closing event
        /// </summary>
        public ICommand CloseCommand => new ActionCommand( CloseWindow );

        private void CloseWindow(object o)
        {
            CheckSelectedPresetBeforeExit();
        }

        #endregion


        // Reserved

        #region ChangeDefaultSoundCommand

        public ICommand ChangeDefaultSoundCommand => new ActionCommand( ChangeDefaultSound );

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
                // TODO:
                //Alarm.SetDefaultSound( ofd.FileName );
            }
        }

        #endregion

        #endregion Commands


        #region methods

        // Sound
        public void Ring(bool stopping = false)
        {
            Alarm.StopDefault();

            if (!stopping) {
                Alarm.PlayDefault();
            }
        }


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

                foreach ( var timePoint in deletingPresetVm.Preset.TimePointCollection ) {
                    Alarm.RemoveSound( timePoint );
                }

                // Collection switches selected preset to null
                PresetViewModelCollection.Remove( deletingPresetVm );

                if (_selectedPreset == null && PresetViewModelCollection.Count > 0) {

                    SelectedPreset = PresetViewModelCollection[0];
                }
            }
            // clear
            else if (e != null && e.OldItems == null && e.NewItems == null) {

                foreach ( var preset in PresetViewModelCollection.Select( p => p.Preset ) ) {
                    foreach ( var timePoint in preset.TimePointCollection ) {
                        Alarm.RemoveSound( timePoint );
                    }
                }

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
            OnPropertyChanged(nameof(IsSelectedPresetExist));
            OnPropertyChanged(nameof(IsNewPreset));
            OnPropertyChanged( nameof( IsPlayable ) );
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
            #region locals

            void DisconnectHandlers( PresetViewModel presetVm )
            {
                _timerManager.TimerStopped -= presetVm.OnTimerStopped;
                _timerManager.TimerPaused -= presetVm.OnTimerPaused;
                _timerManager.SecondPassed -= presetVm.OnSecondPassed;
                _timerManager.TimePointChanged -= presetVm.OnTimePointChanged;

                ((INotifyCollectionChanged)presetVm.TimePointVmCollection).CollectionChanged -= RiseIsPlayableChanged;
                presetVm.PropertyChanged -= OnIsNewPresetChangedHandler;
            }

            void ConnectHandlers()
            {
                _selectedPreset.PropertyChanged += OnIsNewPresetChangedHandler;
                ((INotifyCollectionChanged)_selectedPreset.TimePointVmCollection).CollectionChanged += RiseIsPlayableChanged;

                _timerManager.TimePointChanged += _selectedPreset.OnTimePointChanged;
                _timerManager.SecondPassed += _selectedPreset.OnSecondPassed;
                _timerManager.TimerPaused += _selectedPreset.OnTimerPaused;
                _timerManager.TimerStopped += _selectedPreset.OnTimerStopped;
            }

            #endregion


            if (newSelectedPreset == null) 
            {
                _selectedPreset = null;
                return true;
            }

            var currentPreset = _selectedPreset;

            if (ReferenceEquals(currentPreset, newSelectedPreset)) {
                return false;
            }

            if (currentPreset != null) {
                DisconnectHandlers( currentPreset );
            }

            if ( currentPreset != null 
                 && currentPreset.IsNew 
                 && currentPreset.IsModified 
                 && !ShowSavePresetDialog(currentPreset)) 
            {
                // remove new and not modified last preset
                // After that the selected preset will be try to set himself again
                // and UpdateSelectedPreset will be invoked again.
                _selectedPreset = null;
                _manager.RemovePreset( currentPreset.Preset );
            }

            _selectedPreset = newSelectedPreset;
            _checked = false;
            ConnectHandlers();

            return true;
        }

        private void OnIsNewPresetChangedHandler(object s, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == "IsNew") {

                OnPropertyChanged(nameof(IsNewPreset));
            }
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

        bool IMainViewModel.IsNewPreset( Preset preset )
        {
            return _manager.IsNewPreset( preset );
        }

        private void OnMediaEnded( object s, EventArgs e )
        {
            _isPlayDefault = false;
            Alarm.DefaultMediaEnded -= OnMediaEnded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckSelectedPresetBeforeExit()
        {
            if ( _checked ) {
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

