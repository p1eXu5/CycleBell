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
            AddTimePointCommand = new ActionCommand (_presetViewModel.AddTimePointCommand.Execute, _presetViewModel.AddTimePointCommand.CanExecute);
        }

        public override TimeSpan Time
        {
            get => _timePoint.Time;
            set {
                _timePoint.Time = value;
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
            _timePoint.Name = timePoint.Name;
            _timePoint.Time = timePoint.Time;
            _timePoint.TimePointType = timePoint.TimePointType;
            _timePoint.LoopNumber = timePoint.LoopNumber;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(TimePointType));
            OnPropertyChanged(nameof(LoopNumber));
        }
    }
}
