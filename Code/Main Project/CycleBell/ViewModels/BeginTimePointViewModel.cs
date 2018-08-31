using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels
{
    public class BeginTimePointViewModel : TimePointViewModelBase
    {
        private readonly PresetViewModel _presetViewModel;

        public BeginTimePointViewModel(byte loopNumber, PresetViewModel presetViewModel) : base(TimePoint.MinId, loopNumber)
        {
            _presetViewModel = presetViewModel;
        }
    }
}
