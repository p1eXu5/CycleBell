using System;
using System.IO;
using System.Media;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary.Models;
using Microsoft.Win32;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class TimePointViewModel : TimePointViewModelBase
    {
        #region Const

        private const string DefaultSoundLocation = "defualt sound";

        #endregion

        #region Fields

        private readonly TimePoint _timePoint;
        private bool _isAbsolute;

        private bool _muteFlag;

        private IPresetViewModel _presetViewModel;
        

        #endregion

        #region Constructor

        public TimePointViewModel(TimePoint timePoint, IPresetViewModel presetViewModel) : base(timePoint.Id, timePoint.LoopNumber, presetViewModel)
        {
            _timePoint = timePoint ?? throw new ArgumentNullException();
            _presetViewModel = presetViewModel ?? throw new ArgumentNullException();

            if (_timePoint.Tag is string str) {

                if (String.IsNullOrWhiteSpace (str) || !File.Exists (str))
                    _timePoint.Tag = DefaultSoundLocation;
                else {
                    _PresetViewModel.UpdateSoundBank (this);
                }
            }
        }

        #endregion

        #region Properties

        #region TimePointViewModelBase Overrides

        public override int Id => _timePoint.Id;

        public override byte LoopNumber
        {
            get => _timePoint.LoopNumber;
            set {
                _timePoint.LoopNumber = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Gets TimePoint
        /// </summary>
        public override TimePoint TimePoint => _timePoint;

        /// <summary>
        /// SelectedPresetName of TimePoint
        /// </summary>
        public virtual string Name
        {
            get => _timePoint.Name;
            set {
                _timePoint.Name = value;
                OnPropertyChanged();
            } 
        }

        /// <summary>
        /// Время 'ч' 
        /// </summary>
        public virtual TimeSpan Time
        {
            get {
                if (IsAbsolute) {
                    return  _timePoint.GetAbsoluteTime();
                }
                else {
                    return _timePoint.GetRelativeTime();
                }
            } 
            set {
                _timePoint.Time = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets TimePointTime
        /// </summary>
        public TimePointType TimePointType => _timePoint.TimePointType;

        public bool MuteFlag
        {
            get => _muteFlag;
            set {
                _muteFlag = value;
                OnPropertyChanged ();
            }
        }

        public string SoundLocation => Path.GetFileName((String)TimePoint.Tag);

        public virtual bool IsAbsolute
        {
            get => _isAbsolute;
            set {
                _isAbsolute = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Time));
            }
        }

        #endregion

        #region Commands

        public ICommand RemoveTimePointCommand => new ActionCommand (RemoveTimePoint);
        public ICommand MuteToggleCommand => new ActionCommand (MuteToggle);
        public ICommand AddSoundCommand => new ActionCommand (AddSound);

        #endregion

        #region Methods

        public static implicit operator TimePoint(TimePointViewModel instance) => instance.TimePoint;
        
        /// <summary>
        /// For Button in TimePoint List
        /// </summary>
        /// <param name="o"></param>
        private void RemoveTimePoint(object o)
        {
            _PresetViewModel.RemoveTimePoint (_timePoint);
        }

        private void MuteToggle (object o)
        {
            MuteFlag = (MuteFlag != true);
        }

        private void AddSound (object o)
        {
            OpenWavFile ();
        }

        protected virtual void OpenWavFile()
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Waveform Audio File Format|*.wav" };

            if (ofd.ShowDialog() == true) {

                _timePoint.Tag = ofd.FileName;
                OnPropertyChanged(nameof(SoundLocation));

                _PresetViewModel.UpdateSoundBank (this);
            }
        }

        #endregion
    }
}