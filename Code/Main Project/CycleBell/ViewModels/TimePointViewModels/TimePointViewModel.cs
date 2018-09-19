﻿using System;
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

namespace CycleBell.ViewModels 
{
    public class TimePointViewModel : TimePointViewModelBase
    {
        #region Const

        public const string DefaultSoundName = "default";
        public static readonly SoundPlayer DefaultSoundPlayer; 

        #endregion

        #region Fields

        protected readonly Preset _preset;

        protected TimePoint _timePoint;

        protected SoundPlayer _soundPlayer;
        protected bool _muteFlag = false;

        #endregion

        #region Constructor

        static TimePointViewModel()
        {
            // TODO дебажить фолдеры

                DefaultSoundPlayer = new SoundPlayer(CycleBell.Properties.Resources._default);
        }

        public TimePointViewModel(TimePoint timePoint, Preset preset) : base(timePoint.Id, timePoint.LoopNumber)
        {
            _timePoint = timePoint;
            _preset = preset;

            _soundPlayer = GetSoundPlayer(_timePoint);
        }

        #endregion

        #region Properties

        #region TimePointViewModelBase Overrides

        public override int Id => _timePoint.Id;
        public override byte LoopNumber => _timePoint.LoopNumber;

        #endregion

        /// <summary>
        /// Gets TimePoint
        /// </summary>
        public override TimePoint TimePoint => _timePoint;

        /// <summary>
        /// Name of TimePoint
        /// </summary>
        public string Name
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

        public SoundPlayer SoundPlayer
        {
            get => _soundPlayer;
            set  {
                _soundPlayer = value;
                OnPropertyChanged ();
            }
        }
        public string SoundLocation => _soundPlayer?.SoundLocation;

        #endregion

        #region Commands

        public ICommand RemoveTimePointCommand => new ActionCommand (RemoveTimePoint);
        public ICommand MuteToggleCommand => new ActionCommand (MuteToggle, CanMuteToggle);
        public ICommand RingCommand => new ActionCommand (Ring, CanRing);
        
        // Calls std dialog
        public ICommand AddSoundCommand => new ActionCommand (AddSound);

        #endregion

        #region Methods

        public void Reset()
        {
            Time = TimeSpan.Zero;
        }

        public static implicit operator TimePoint(TimePointViewModel instance) => instance.TimePoint;

        /// <summary>
        /// Return TimePoint SoundPlayer or DefaultSoundPlayer
        /// </summary>
        /// <param name="timePoint"></param>
        /// <returns></returns>
        private SoundPlayer GetSoundPlayer (TimePoint timePoint)
        {
            if (String.IsNullOrWhiteSpace ((string) timePoint.Tag)) {

                timePoint.Tag = DefaultSoundName;
                return DefaultSoundPlayer;
            }

            if ((string) timePoint.Tag == DefaultSoundName)
                return DefaultSoundPlayer;

            if (File.Exists ((string)timePoint.Tag))
                return new SoundPlayer((string)timePoint.Tag);

            return null;
        }
        
        /// <summary>
        /// For Button in TimePoint List
        /// </summary>
        /// <param name="o"></param>
        private void RemoveTimePoint(object o)
        {
            _preset.RemoveTimePoint (_timePoint);
        }

        private void MuteToggle (object o)
        {
            MuteFlag = (MuteFlag != true);
        }
        private bool CanMuteToggle (object o)
        {
            return _soundPlayer != null;
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