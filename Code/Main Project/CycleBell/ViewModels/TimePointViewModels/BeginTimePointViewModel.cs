using System;
using CycleBellLibrary.Models;
using CycleBellLibrary.Repository;

namespace CycleBell.ViewModels.TimePointViewModels
{
    public class BeginTimePointViewModel : TimePointViewModelBase
    {
        public BeginTimePointViewModel(byte loopNumber, IPresetViewModel presetViewModel) : base((int)TimePoint.MinId, loopNumber, presetViewModel)
        { }

        public override TimePoint TimePoint => null;

        public string CycleName => $"loop {LoopNumber}";

        public int LoopCount
        {
            get => _PresetViewModel.Preset.TimerLoops[LoopNumber];
            set {
                _PresetViewModel.Preset.TimerLoops[LoopNumber] = value;
                OnPropertyChanged();
            }
        }
    }
}
