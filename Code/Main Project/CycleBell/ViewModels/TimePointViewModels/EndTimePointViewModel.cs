using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class EndTimePointViewModel : TimePointViewModelBase
    {
        public EndTimePointViewModel(byte loopNumber) : base((int)TimePoint.MaxId, loopNumber)
        {
        }

        public override TimePoint TimePoint => throw new NotImplementedException();
    }
}
