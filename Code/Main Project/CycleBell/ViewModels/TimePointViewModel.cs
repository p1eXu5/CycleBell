using System;
using System.Media;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary;
using Microsoft.Win32;

namespace CycleBell.ViewModels 
{
    public class TimePointViewModel : TimePointViewModelBase
    {
        #region Fields

        private static SoundPlayer _sound = new SoundPlayer("pack://application:,,,/Sounds/default.wav");

        private readonly TimePoint _timePoint;

        #endregion

        #region Constructor

        public TimePointViewModel(TimePoint timePoint)
        {
            _timePoint = timePoint;
        }

        #endregion

        #region Properties

        #region TimePointViewModelBase Overrides

        public override int Id => _timePoint.Id;
        public override byte LoopNumber => _timePoint.LoopNumber;

        #endregion

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
        public TimeSpan Time
        {
            get => _timePoint.Time;
            set {
                _timePoint.Time = value;
                OnPropertyChanged();
            }
        }

        public bool IsAbsoluteTime
        {
            get => _timePoint.TimePointType == TimePointType.Absolute;
        }

        /// <summary>
        /// Sound location
        /// </summary>
        public string SoundLocation => String.IsNullOrEmpty((string)_timePoint?.Tag) ? (string)_timePoint?.Tag : _sound.SoundLocation;

        #endregion

        #region Commands

        public ICommand AddSoundCommand { get; }

        #endregion

        #region Methods

        private static void OpenWavFile(SoundPlayer sound)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Waveform Audio File Format|*.wav"
            };

            if (ofd.ShowDialog() == true) {
                sound.SoundLocation = ofd.FileName;
            }
        }

        #endregion
    }
}