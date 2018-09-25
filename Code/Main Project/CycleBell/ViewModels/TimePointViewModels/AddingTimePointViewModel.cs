using System;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class AddingTimePointViewModel : TimePointViewModel
    {
        public AddingTimePointViewModel (IPresetViewModel presetViewModel) : base (new TimePoint { Name = "" }, presetViewModel)
        {
            AddTimePointCommand = new ActionCommand (_PresetViewModel.AddTimePointCommand.Execute, _PresetViewModel.AddTimePointCommand.CanExecute);
        }

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

        public ICommand AddTimePointCommand { get; }

        public void Reset()
        {
            Name = "";
            Time = TimeSpan.Zero;
            TimePointType = TimePointType.Relative;
            LoopNumber = 0;
        }

        public void CopyFrom(TimePoint timePoint)
        {
            _TimePoint.Name = timePoint.Name;
            _TimePoint.Time = timePoint.Time;
            _TimePoint.TimePointType = timePoint.TimePointType;
            _TimePoint.LoopNumber = timePoint.LoopNumber;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(TimePointType));
            OnPropertyChanged(nameof(LoopNumber));
        }
    }
}
