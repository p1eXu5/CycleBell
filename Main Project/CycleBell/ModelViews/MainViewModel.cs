using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CycleBell.Annotations;
using CycleBell.Base;
using CycleBell.Models;
using CycleBellLibrary;

namespace CycleBell.ModelViews
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private

        private ICycleBellTimerManager _manager;

        private CycleBellStateFlags _cycleBellState;
        private IDialogRegistrator _dialogRegistrator;
        private PresetViewModel _selectedPreset;
        private ObservableCollection<PresetViewModel> _presets;

        #endregion Private

        #region Constructor

        public MainViewModel(IDialogRegistrator dialogRegistrator, ICycleBellTimerManager cbtm)
        {
            _dialogRegistrator = dialogRegistrator;
            _manager = cbtm;

            _presets = new ObservableCollection<PresetViewModel>(_manager.Presets.Select(p => new PresetViewModel(p)));
            _manager.PresetCollectionChanged += (s, e) =>
                                                {
                                                    // TODO:

                                                };
        }

        #endregion Constructor

        #region CLR Properties

        public CycleBellStateFlags FirstCallState 
        {
            get => _cycleBellState & CycleBellStateFlags.FirstCall;
            set {
                _cycleBellState = value & CycleBellStateFlags.FirstCall | InfiniteLoopState;
                OnPropertyChanged (nameof(FirstCallState));
            }
        }

        public CycleBellStateFlags InfiniteLoopState 
        { 
            get => _cycleBellState & CycleBellStateFlags.InfiniteLoop;
            set {
                 _cycleBellState = value & CycleBellStateFlags.InfiniteLoop | FirstCallState;
                OnPropertyChanged (nameof(InfiniteLoopState));
            }
        }

        public List<PresetViewModel> Presets
        {
            get => _presets;
            set {
                _presets = value;
                OnPropertyChanged(nameof(Presets));
            }
        }
        public PresetViewModel SelectedPreset
        {
            get => _selectedPreset;
            set {
                _selectedPreset = value;
                OnPropertyChanged(nameof(SelectedPreset));
                OnPropertyChanged(nameof(StartTime));
            }
        }

        public TimeSpan StartTime
        {
            get => _selectedPreset?.StartTime ?? TimeSpan.Zero;
            set {
                if (_selectedPreset != null) {

                    _selectedPreset.StartTime = value;
                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        #endregion CLR Properties

        #region Commands

        public ActionCommand NewCommand => new ActionCommand(NewPresets);
        public ActionCommand OpenCommand => new ActionCommand(OpenPresets);
        public ActionCommand SaveCommand => new ActionCommand(SavePresets);
        public ActionCommand SaveAsCommand => new ActionCommand(SaveAsPresets);
        public ActionCommand ExitCommand => new ActionCommand(Exit);
        public ActionCommand SoundSelectCommand => new ActionCommand(SoundSelect);

        public ActionCommand FirstCallCommand => new ActionCommand (p =>
                                                    {
                                                        CycleBellStateFlags state = _cycleBellState & CycleBellStateFlags.FirstCall;
                                                        FirstCallState = (byte) state == 0 ? CycleBellStateFlags.FirstCall : 0;
                                                    });

            
        public ActionCommand InfiniteLoopCommand => new ActionCommand (p =>
                                                    {
                                                        CycleBellStateFlags state = _cycleBellState & CycleBellStateFlags.InfiniteLoop;
                                                        InfiniteLoopState = (byte) state == 0 ? CycleBellStateFlags.InfiniteLoop : 0;
                                                    });

        public ActionCommand ViewHelpCommand => new ActionCommand(About);
        public ActionCommand AboutCommand => new ActionCommand(About);

        #endregion Commands

        #region Methods

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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
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
