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

        public BeginTimePointViewModel(byte loopNumber, Preset preset) : base((int)TimePoint.MinId, loopNumber)
        {
            _preset = preset;
        }

        public override TimePoint TimePoint => throw new NotImplementedException();

        public string CycleName => $"loop {LoopNumber}";

        public int NumberOfLoops
        {
            get => _preset.TimerLoops[LoopNumber];
            set {
                _preset.TimerLoops[LoopNumber] = value;
                OnPropertyChanged();
            }
        }
    }
}
