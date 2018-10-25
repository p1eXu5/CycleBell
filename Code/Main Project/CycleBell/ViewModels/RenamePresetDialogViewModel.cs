﻿using System;
using CycleBellLibrary.Models;

namespace CycleBell.ViewModels
{
    public class RenamePresetDialogViewModel : DialogViewModelBase
    {
        private readonly Preset _preset;

        public RenamePresetDialogViewModel(IPresetViewModel presetViewModel)
        {
            _preset = presetViewModel?.Preset ?? throw new ArgumentNullException();
        }

        public string PresetName
        {
            get => _preset.PresetName;
            set {
                _preset.PresetName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoName));
                OnPropertyChanged(nameof(HasName));
            }
        }

        public bool HasNoName => String.IsNullOrWhiteSpace(PresetName);
        public bool HasName => !HasNoName;

    }
}
