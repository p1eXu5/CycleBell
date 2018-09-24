using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CycleBell.Base;
using CycleBellLibrary;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;
using Microsoft.Win32;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class TimePointViewModel : TimePointViewModelBase
    {
        #region Const

        private const string DefaultSoundLocation = "defualt sound";

        #endregion

        #region Fields

        protected readonly IPresetViewModel _presetViewModel;

        protected TimePoint _timePoint;

        protected SoundPlayer _soundPlayer;
        protected bool _muteFlag = false;

        #endregion

        #region Constructor

        public TimePointViewModel(TimePoint timePoint, IPresetViewModel presetViewModel) : base(timePoint.Id, timePoint.LoopNumber)
        {
            _timePoint = timePoint;
            _presetViewModel = presetViewModel;

            if (_timePoint.Tag is string str) {

                if (String.IsNullOrWhiteSpace (str) || !File.Exists (str))
                    _timePoint.Tag = DefaultSoundLocation;
                else {
                    _presetViewModel.UpdateSoundBank (this);
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
        /// Name of TimePoint
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
            get => _timePoint.Time;
            set {
                _timePoint.Time = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets TimePointTime
        /// </summary>
        public TimePointType TimePointType
        {
            get => _timePoint.TimePointType;
            set {
                _timePoint.TimePointType = value;
                OnPropertyChanged();
            }
        }

        public bool IsAbsoluteTime
        {
            get => _timePoint.TimePointType == TimePointType.Absolute;
        }

        public bool MuteFlag
        {
            get => _muteFlag;
            set {
                _muteFlag = value;
                OnPropertyChanged ();
            }
        }

        public string SoundLocation => (String)TimePoint.Tag;

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
            _presetViewModel.RemoveTimePoint (_timePoint);
        }

        private void MuteToggle (object o)
        {
            MuteFlag = (MuteFlag != true);
        }


        private void Ring (object o)
        {
            _soundPlayer.Play();
        }
        private bool CanRing (object o)
        {
            return (_soundPlayer != null) && !MuteFlag;
        }

        private void AddSound (object o)
        {
            OpenWavFile (_soundPlayer);
            _presetViewModel.UpdateSoundBank (this);
        }

        private void OpenWavFile(SoundPlayer sound)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Waveform Audio File Format|*.wav" };

            if (ofd.ShowDialog() == true) {

                sound.SoundLocation = ofd.FileName;
                _timePoint.Tag = ofd.FileName;
            }
        }

        #endregion
    }
}