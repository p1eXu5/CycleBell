using System;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels.TimePointViewModels
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
