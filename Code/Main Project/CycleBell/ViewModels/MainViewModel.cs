using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using CycleBellLibrary;

using CycleBell.Annotations;
using CycleBell.Base;
using CycleBellLibrary.Context;
using CycleBellLibrary.Repository;
using CycleBellLibrary.Timer;

namespace CycleBell.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region Private

        private readonly IDialogRegistrator _dialogRegistrator;

        private readonly ICycleBellManager _manager;
        private readonly IPresetsManager _presetManager;
        private readonly ITimerManager _timerManager;

        private PresetViewModel _selectedPreset;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellManager cbm)
        {
            _dialogRegistrator = dialogRegistrator;
            _manager = cbm;
            _presetManager = cbm.PresetsManager;
            _timerManager = cbm.TimerManager;

            Presets = new ObservableCollection<PresetViewModel>(_presetManager.Presets.Select(p => new PresetViewModel(p)));
            ((INotifyCollectionChanged)(_presetManager.Presets)).CollectionChanged += (s, e) =>
                                                {
                                                    // TODO:
                                                    if (e.NewItems[0] != null)
                                                        Presets.Add(new PresetViewModel((Preset)e.NewItems[0]));
                                                };

        }

        #endregion Constructor

        #region CLR Properties

        public ObservableCollection<PresetViewModel> Presets { get; set; }

        public PresetViewModel SelectedPreset
        {
            get => _selectedPreset;
            set {
                _selectedPreset = value;
                OnPropertyChanged ();
            }
        }

        //public CycleBellStateFlags FirstCallState 
        //{
        //    get => _cycleBellState & CycleBellStateFlags.FirstCall;
        //    set {
        //        _cycleBellState = value & CycleBellStateFlags.FirstCall | InfiniteLoopState;
        //        OnPropertyChanged (nameof(FirstCallState));
        //    }
        //}

        //public CycleBellStateFlags InfiniteLoopState 
        //{ 
        //    get => _cycleBellState & CycleBellStateFlags.InfiniteLoop;
        //    set {
        //         _cycleBellState = value & CycleBellStateFlags.InfiniteLoop | FirstCallState;
        //        OnPropertyChanged (nameof(InfiniteLoopState));
        //    }
        //}


        //public PresetViewModel SelectedPreset
        //{
        //    get => _selectedPreset;
        //    set {
        //        _selectedPreset = value;
        //        OnPropertyChanged(nameof(SelectedPreset));
        //        OnPropertyChanged(nameof(StartTime));
        //    }
        //}

        //public TimeSpan StartTime
        //{
        //    get => _selectedPreset?.StartTime ?? TimeSpan.Zero;
        //    set {
        //        if (_selectedPreset != null) {

        //            _selectedPreset.StartTime = value;
        //            OnPropertyChanged(nameof(StartTime));
        //        }
        //    }
        //}

        #endregion CLR Properties

        #region Commands

        public ICommand CreatePresetCommand => new ActionCommand(CreatePreset);
        public ICommand NewCommand => new ActionCommand(NewPresets);
        public ICommand OpenCommand => new ActionCommand(OpenPresets);
        public ICommand SaveCommand => new ActionCommand(SavePresets);
        public ICommand SaveAsCommand => new ActionCommand(SaveAsPresets);
        public ICommand ExitCommand => new ActionCommand(Exit);
        public ICommand SoundSelectCommand => new ActionCommand(SoundSelect);

        //public ActionCommand FirstCallCommand => new ActionCommand (p =>
        //                                            {
        //                                                CycleBellStateFlags state = _cycleBellState & CycleBellStateFlags.FirstCall;
        //                                                FirstCallState = (byte) state == 0 ? CycleBellStateFlags.FirstCall : 0;
        //                                            });

            
        //public ActionCommand InfiniteLoopCommand => new ActionCommand (p =>
        //                                            {
        //                                                CycleBellStateFlags state = _cycleBellState & CycleBellStateFlags.InfiniteLoop;
        //                                                InfiniteLoopState = (byte) state == 0 ? CycleBellStateFlags.InfiniteLoop : 0;
        //                                            });

        public ActionCommand ViewHelpCommand => new ActionCommand(About);
        public ActionCommand AboutCommand => new ActionCommand(About);

        #endregion Commands

        #region Methods

        private void CreatePreset (object obj)
        {
            _manager.CreateNewPreset();
        }

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

        private void SaveAsPresets(object obj)
        {
            // TODO:
        }

        private void Exit(object obj)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SoundSelect(object obj)
        {
            throw new NotImplementedException();
        }

        private void About(object obj)
        {
            var viewModel = new AboutDialogViewModel();
            bool? res = _dialogRegistrator.ShowDialog(viewModel);

            ;

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
