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

        protected SoundPlayer _SoundPlayer;
        protected bool _MuteFlag = false;

        private bool _active;

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
        /// Name of TimePoint
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
            get => _TimePoint.Time;
            set {
                _TimePoint.Time = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets TimePointTime
        /// </summary>
        public TimePointType TimePointType
        {
            get => _TimePoint.TimePointType;
            set {
                _TimePoint.TimePointType = value;
                OnPropertyChanged();
            }
        }

        public bool IsAbsoluteTime
        {
            get => _TimePoint.TimePointType == TimePointType.Absolute;
        }

        public bool MuteFlag
        {
            get => _MuteFlag;
            set {
                _MuteFlag = value;
                OnPropertyChanged ();
            }
        }

        public string SoundLocation => (String)TimePoint.Tag;

        public bool Active
        {
            get => _active;
            set {
                _active = value;
                OnPropertyChanged();
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
            _PresetViewModel.UpdateSoundBank (this);
        }

        private void OpenWavFile(SoundPlayer sound)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Waveform Audio File Format|*.wav" };

            if (ofd.ShowDialog() == true) {

                sound.SoundLocation = ofd.FileName;
                _TimePoint.Tag = ofd.FileName;
            }
        }

        #endregion
    }
}