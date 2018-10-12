using System;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary.Models;
using Microsoft.Win32;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class AddingTimePointViewModel : TimePointViewModel
    {
        private const byte _loopNumberLimit = 10;
        private bool _focusTime;
        private bool _focusName;

        public AddingTimePointViewModel (IPresetViewModel presetViewModel) : base (new TimePoint { Name = "" }, presetViewModel)
        {
            AddTimePointCommand = new ActionCommand (AddTimePoint, _PresetViewModel.AddTimePointCommand.CanExecute);

            NumberCollection = _PresetViewModel.Preset != null 
                               && _PresetViewModel.Preset.TimerLoops.Values.Any() 
                               && _PresetViewModel.Preset.TimerLoops.Values.Max() > _loopNumberLimit 
                                    ? Enumerable.Range (0, _PresetViewModel.Preset.TimerLoops.Values.Max() + 1).Select (n => (byte) n).ToArray() 
                                    : Enumerable.Range(0, _loopNumberLimit).Select(n => (byte)n).ToArray();
        }

        #region Properties

        public override TimeSpan Time
        {
            get => _TimePoint.Time;
            set {
                _TimePoint.Time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoSetTime));
                ((ActionCommand)AddTimePointCommand).RaiseCanExecuteChanged();
            }
        }

        public override string Name
        {
            get => base.Name;
            set {
                base.Name = value;
                OnPropertyChanged(nameof(HasNoName));
            }
        }

        public bool HasNoName => String.IsNullOrWhiteSpace(Name);
        public bool NoSetTime => Time == TimeSpan.Zero && TimePointType == TimePointType.Relative;

        public bool FocusTime
        {
            get => _focusTime;
            set {
                _focusTime = value;
                OnPropertyChanged();
            }
        }

        public bool FocusName
        {
            get => _focusName;
            set {
                _focusName = value;
                OnPropertyChanged();
            }
        }

        public byte[] NumberCollection { get; private set; }

        #endregion

        #region Commands

        public ICommand AddTimePointCommand { get; }
        public ICommand TimePointNameReturnCommand => new ActionCommand(TimePointNameReturn);

        #endregion

        private void TimePointNameReturn(object o)
        {
            FocusTime ^= true;
        }

        private void AddTimePoint(object o)
        {
            _PresetViewModel.AddTimePointCommand.Execute(null);
            FocusName ^= true;
        }

        public void Reset()
        {
            Name = "";
            Time = TimeSpan.Zero;

            _TimePoint.ChangeTimePointType(TimePointType.Relative);
            OnPropertyChanged(nameof(TimePointType));

            LoopNumber = 0;
        }

        public void CopyFrom(TimePoint timePoint)
        {
            _TimePoint.Name = timePoint.Name;
            _TimePoint.Time = timePoint.Time;
            _TimePoint.ChangeTimePointType(timePoint.TimePointType);
            _TimePoint.LoopNumber = timePoint.LoopNumber;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(TimePointType));
            OnPropertyChanged(nameof(LoopNumber));
        }

        protected override void OpenWavFile(SoundPlayer sound)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Waveform Audio File Format|*.wav" };

            if (ofd.ShowDialog() == true) {

                sound.SoundLocation = ofd.FileName;
                _TimePoint.Tag = ofd.FileName;
                OnPropertyChanged(nameof(SoundLocation));
            }
        }

    }
}
