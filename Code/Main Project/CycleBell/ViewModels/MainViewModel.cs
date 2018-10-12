﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Data;
using System.Windows.Input;

using CycleBell.Base;
using CycleBell.ViewModels.TimePointViewModels;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;
using Microsoft.Win32;

namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        #region Fields

        public static SoundPlayer DefaultSoundPlayer = new SoundPlayer("..\\Code\\Main Project\\CycleBell\\Sounds\\default.wav"); 

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private readonly ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;
        private PresetViewModel _prevSelectedPreset;

        private string _initialDirectory = null;

        private bool _isNewPreset;
        private bool _isRingOnStartTime = true;
        private bool _isFocused = false;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cycleBellManager)
        {
            _dialogRegistrator = dialogRegistrator ?? throw new ArgumentNullException(nameof(dialogRegistrator));

            _manager = cycleBellManager ?? throw new ArgumentNullException(nameof(cycleBellManager));
            _manager.CantCreateNewPresetEvent += OnCantCreateNewPresetEventHandler;

            var presetManager = cycleBellManager.PresetCollectionManager;
            PresetViewModelCollection = new ObservableCollection<PresetViewModel>(presetManager.Presets.Select(p => new PresetViewModel(p, this)));

            ((INotifyCollectionChanged) (presetManager.Presets)).CollectionChanged += OnPresetCollectionChangedEventHandler;

            _timerManager = cycleBellManager.TimerManager;
            _timerManager.TimerStartEvent += UpdateIsRunningAndTimerStateProperties;
            _timerManager.TimerStopEvent += UpdateIsRunningAndTimerStateProperties;

            AppDomain.CurrentDomain.ProcessExit += (s, e) => SavePresetsBeforeExit (null);

            RemoveSelectedPresetCommand = new ActionCommand(RemoveSelectedPreset, CanRemoveSelectedPreset);
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

                _selectedPreset = value;

                if (newSelectedPreset != null && _prevSelectedPreset != null && _prevSelectedPreset.Name == Preset.DefaultName)
                    _manager.CheckCreateNewPreset(_prevSelectedPreset.Preset);

                OnPropertyChanged();

                OnPropertyChanged(nameof(IsInfiniteLoop));
                OnPropertyChanged(nameof(HasNoName));
                OnPropertyChanged(nameof(IsSelectedPreset));
                ((ActionCommand)RemoveSelectedPresetCommand).RaiseCanExecuteChanged();
            }
        }

        public string SelectedPresetName
        {
            get => _selectedPreset?.Name;
            set {
                if (value != "")
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

                    return false;
                }
            }
        }

        public bool IsRunning => _timerManager.IsRunning;

        public bool IsPaused => _timerManager.IsPaused;
        public bool IsStopped
        {
            get => IsRunning == false && IsPlayable;
            set {
                if (IsRunning) {

                    _timerManager.Stop();
                    OnPropertyChanged();
                }
            }
        }

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

            #region menu
        public ICommand CreateNewPresetCommand => new ActionCommand(CreateNewPreset);

        public ICommand SavePresetCommand => new ActionCommand(SavePreset, CanSavePreset);
        public ICommand SavePresetAsCommand => new ActionCommand(SavePresetAs, CanSavePresetAs);

        public ICommand ImportPresetsCommand => new ActionCommand(ImportPresets);
        public ICommand ExportPresetsCommand => new ActionCommand(ExportPresets, CanExportPresets);

        public ICommand ExitCommand => new ActionCommand(Exit);

        public ICommand InfiniteLoopCommand => new ActionCommand((o) => { IsInfiniteLoop ^= true; });

        public ICommand SavePresetsBeforeExitCommand => new ActionCommand(SavePresetsBeforeExit);

        public ICommand ViewHelpCommand => new ActionCommand(About, o => false);
        public ICommand AboutCommand => new ActionCommand(About);
            #endregion

        public ICommand StopCommand => new ActionCommand (Stop);
        public ICommand RingOnStartTimeSwitchCommand => new ActionCommand (SwitchIsRingOnStartTime);
        public ICommand RingCommand => new ActionCommand(Ring);
        public ICommand PresetComboBoxReturnCommand => new ActionCommand (PresetComboBoxReturnKeyHandler);
        public ICommand RemoveSelectedPresetCommand { get; }

        public ICommand MediaTerminalCommand => new ActionCommand (MediaTerminal);

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
            if (e?.NewItems?[0] != null && e.OldItems?[0] == null) { 

                PresetViewModelCollection.Add(new PresetViewModel((Preset)e.NewItems[0], this));
                SelectedPreset = PresetViewModelCollection[PresetViewModelCollection.Count - 1];

                ConnectHandlers(SelectedPreset);
            }

            if (e?.OldItems?[0] != null && e?.NewItems?[0] == null) {

                var deletingPresetVm = PresetViewModelCollection.First(pvm => pvm.Preset.Equals((Preset)e.OldItems[0]));

                DisconnectHandlers(SelectedPreset);

                PresetViewModelCollection.Remove(deletingPresetVm);
                //if (!PresetViewModelCollection.IsNotify) {
                //    PresetViewModelCollection.IsNotify = true;
                //}

                //SelectedPreset = PresetViewModelCollection.Count > 0 ? PresetViewModelCollection[0] : null;
            }

            OnPropertyChanged(nameof(SelectedPreset));
            OnPropertyChanged(nameof(IsNewPreset));
        }
        
        private void OnCantCreateNewPresetEventHandler(object sender, CantCreateNewPreetEventArgs args)
        {
            if (args.CantCreateNewPresetReason == CantCreateNewPresetReasons.EmptyPresetNotModified) {

                //PresetViewModelCollection.IsNotify = false;
                _manager.DeletePreset(args.Preset);
                return;
            }

            var saveViewModel = new SavePresetDialogViewModel();

            if (_dialogRegistrator.ShowDialog(saveViewModel) == true) {

                var renameViewModel = new RenamePresetDialogViewModel(_prevSelectedPreset);

                _dialogRegistrator.ShowDialog(renameViewModel);
                _prevSelectedPreset.Name = _prevSelectedPreset.Name;
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
            _timerManager.TimerStopEvent += selectedPresetViewModel.OnTimerStopEventHandler;
        }

        /// <summary>
        /// Disconnect handlers from SelectedPreset
        /// </summary>
        private void DisconnectHandlers(PresetViewModel selectedPresetViewModel)
        {
            _timerManager.TimerStopEvent -= selectedPresetViewModel.OnTimerStopEventHandler;
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

        //  Create the new preset
        private void CreateNewPreset(object obj)
        {
            _manager.CreateNewPreset();
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

                    CreateNewPreset(null);
                }
            }
        }
        private bool CanSavePresetAs(object obj) => _selectedPreset?.IsModified ?? false;

        //  Import/Export PresetViewModelCollection
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
            return PresetViewModelCollection.Count > 0;
        }

        //  Save presets before exit
        private void SavePresetsBeforeExit(object obj)
        {
             _manager.SavePresets();
        }


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
                OnPropertyChanged(nameof(TimerState));
            }
            else {
                // if not running
                _timerManager.PlayAsync (_selectedPreset.Preset);
            }

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
            DefaultSoundPlayer.Play();
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


        private void PresetComboBoxReturnKeyHandler (object newName)
        {
            //SelectedPreset.Name = newName.ToString();
            OnPropertyChanged(nameof(HasNoName));

            // Change value for call PropertyChangedCallback in attached property
            IsFocused ^= true;
        }

        #endregion Methods

        #region IMainViewModel impl

        public void Ring() => Ring(null);
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

