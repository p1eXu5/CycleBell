using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                ((ActionCommand)AddTimePointCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand AddTimePointCommand { get; }


    }
}
