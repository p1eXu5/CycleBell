using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels
{
    public class BeginTimePointViewModel : TimePointViewModelBase
    {
        private readonly Preset _preset;

        public BeginTimePointViewModel(byte loopNumber, Preset preset) : base(TimePoint.MinId, loopNumber)
        {
            _preset = preset;
        }

        public override TimePoint TimePoint => throw new NotImplementedException();
    }
}
