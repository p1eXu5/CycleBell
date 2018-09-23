using System;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class AddingTimePointViewModel : TimePointViewModel
    {
        public AddingTimePointViewModel(TimePoint timePoint, Preset preset, ICommand addTimePointCommand) :
            base(timePoint, preset)
        {
            AddTimePointCommand = addTimePointCommand;
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
            Time = TimeSpan.Zero;
        }
    }
}
