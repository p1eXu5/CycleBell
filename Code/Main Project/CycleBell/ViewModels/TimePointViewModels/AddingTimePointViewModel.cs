using System;
using System.Windows.Input;
using CycleBell.Base;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class AddingTimePointViewModel : TimePointViewModel
    {
        public AddingTimePointViewModel(IPresetViewModel presetViewModel) : base(new TimePoint { Name = "" }, presetViewModel)
        { }

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

        public ICommand AddTimePointCommand => _presetViewModel.AddTimePointCommand;

        public void Reset()
        {
            Time = TimeSpan.Zero;
        }
    }
}
