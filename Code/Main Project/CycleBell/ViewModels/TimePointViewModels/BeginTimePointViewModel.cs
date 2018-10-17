using System;
using System.ComponentModel.DataAnnotations;
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

        [Range(1, Int32.MaxValue, ErrorMessage = "Loop count must be greater then 0")]
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
