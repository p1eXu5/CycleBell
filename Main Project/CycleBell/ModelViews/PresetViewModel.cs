using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CycleBell.Base;
using CycleBell.Models;
using Microsoft.Win32;

namespace CycleBell.ModelViews
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
        private List<TimePointViewModel> _timePoints;

        public PresetViewModel()
        {
            _timePoints = new List<TimePointViewModel>();
        }

        public string Name { get; set; }
        public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 5, 0);
        public CycleBellStateFlags State { get; set; }
        public List<TimePointViewModel> TimePoints { get; }
    }

    public class TimePointViewModel : Notifyer
    {
        private PresetViewModel _preset;
        private TimePoint _timePoint;

        public TimePointViewModel(PresetViewModel preset, TimePoint timePoint)
        {
            _timePoint = timePoint;
            _preset = preset;
            SoundPlayer sound = new SoundPlayer("pack://application:,,,/Sounds/default.wav");

            AddSoundCommand = new ActionCommand(p => { OpenWavFile(_timePoint.Sound); });
        }

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
        public string SoundLocation => _timePoint.Sound.SoundLocation;

        public ICommand AddSoundCommand { get; }

        private static void OpenWavFile(SoundPlayer sound)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Waveform Audio File Format|*.wav";

            if (ofd.ShowDialog() == true) {
                sound.SoundLocation = ofd.FileName;
            }
        }
    }
}
