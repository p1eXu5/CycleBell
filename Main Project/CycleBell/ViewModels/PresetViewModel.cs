using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Media;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary;

namespace CycleBell.ViewModels
{
    [Flags]
    public enum CycleBellStateFlags : byte
    {
        FirstCall = 0x01,
        InfiniteLoop = 0x02
    }

    /// <summary>
    /// Timer preset
    /// </summary>
    public class PresetViewModel
    {
        private readonly Preset _preset;
        private readonly ObservableCollection<TimePointViewModelBase> _timePoints;

        #region Constructor

        public PresetViewModel(Preset preset)
        {
            _preset = preset;

            _timePoints = new ObservableCollection<TimePointViewModelBase>(_preset.TimePoints.Select(t => new TimePointViewModel(t)));
            TimePoints = new ReadOnlyObservableCollection<TimePointViewModelBase>(_timePoints);

            ((INotifyCollectionChanged) _preset.TimePoints).CollectionChanged += (s, e) => { };
        }

        #endregion

        public string Name { get; set; }
        public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 5, 0);
        public CycleBellStateFlags State { get; set; }

        public ReadOnlyObservableCollection<TimePointViewModelBase> TimePoints { get; }
        public TimerLoopSortedDictionary TimerLoops => _preset.TimerLoops;
    }

    public class TimePointViewModelBase : Notifyer
    {
        #region Fields

        private int _id;
        private byte _loopNumber;

        #endregion
        
        #region Constructors

        protected TimePointViewModelBase() {}

        public TimePointViewModelBase (int id, byte loopNumber)
        {
            _id = id;
            _loopNumber = loopNumber;
        }

        #endregion

        #region Properties

        public virtual int Id
        {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged();
            }
        }

        public virtual byte LoopNumber
        {
            get => _loopNumber;
            set {
                _loopNumber = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
    }

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

            AddSoundCommand = new ActionCommand(p => { OpenWavFile(_timePoint.Sound); });
        }

        #endregion

        #region Properties

        #region TimePointViewModelBase Overrides

        public override int Id => _timePoint.Id;
        public override byte LoopNumber => _timePoint.LoopNumber;

        #endregion

        /// <summary>
        /// Время 'ч' 
        /// </summary>
        public TimeSpan Time
        {
            get => _timePoint.Time;
            set {
                _timePoint.Time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        /// <summary>
        /// Name of bell
        /// </summary>
        public string Name
        {
            get => _timePoint.Name;
            set {
                _timePoint.Name = value;
                OnPropertyChanged(nameof(Name));
            } 
        }

        /// <summary>
        /// Times segment
        /// </summary>
        public byte TimeSection
        {
            get => _timePoint.TimeSection;
            set {
                _timePoint.TimeSection = value;
                OnPropertyChanged(nameof(TimeSection));
            }
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Waveform Audio File Format|*.wav";

            if (ofd.ShowDialog() == true) {
                sound.SoundLocation = ofd.FileName;
            }
        }

        #endregion
    }
}
