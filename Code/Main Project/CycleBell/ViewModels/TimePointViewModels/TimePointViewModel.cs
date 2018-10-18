using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Input;
using System.Windows.Navigation;
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

        protected readonly TimePoint _TimePoint;

        protected SoundPlayer _SoundPlayer = new SoundPlayer();
        protected bool _MuteFlag = false;
        protected bool _isAbsolute; 

        #endregion

        #region Constructor

        public TimePointViewModel(TimePoint timePoint, IPresetViewModel presetViewModel) : base(timePoint.Id, timePoint.LoopNumber, presetViewModel)
        {
            _TimePoint = timePoint;

            if (_TimePoint.Tag is string str) {

                if (String.IsNullOrWhiteSpace (str) || !File.Exists (str))
                    _TimePoint.Tag = DefaultSoundLocation;
                else {
                    _PresetViewModel.UpdateSoundBank (this);
                }
            }
        }

        #endregion

        #region Properties

        #region TimePointViewModelBase Overrides

        public override int Id => _TimePoint.Id;

        public override byte LoopNumber
        {
            get => _TimePoint.LoopNumber;
            set {
                _TimePoint.LoopNumber = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Gets TimePoint
        /// </summary>
        public override TimePoint TimePoint => _TimePoint;

        /// <summary>
        /// SelectedPresetName of TimePoint
        /// </summary>
        public virtual string Name
        {
            get => _TimePoint.Name;
            set {
                _TimePoint.Name = value;
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
                    return  _TimePoint.GetAbsoluteTime();
                }
                else {
                    return _TimePoint.GetRelativeTime();
                }
            } 
            set {
                _TimePoint.Time = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets TimePointTime
        /// </summary>
        public TimePointType TimePointType => _TimePoint.TimePointType;

        public bool MuteFlag
        {
            get => _MuteFlag;
            set {
                _MuteFlag = value;
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
            _PresetViewModel.RemoveTimePoint (_TimePoint);
        }

        private void MuteToggle (object o)
        {
            MuteFlag = (MuteFlag != true);
        }


        private void Ring (object o)
        {
            _SoundPlayer.Play();
        }
        private bool CanRing (object o)
        {
            return (_SoundPlayer != null) && !MuteFlag;
        }

        private void AddSound (object o)
        {
            OpenWavFile (_SoundPlayer);
        }

        protected virtual void OpenWavFile(SoundPlayer sound)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Waveform Audio File Format|*.wav" };

            if (ofd.ShowDialog() == true) {

                sound.SoundLocation = ofd.FileName;
                _TimePoint.Tag = ofd.FileName;
                OnPropertyChanged(nameof(SoundLocation));

                _PresetViewModel.UpdateSoundBank (this);
            }
        }

        #endregion
    }
}