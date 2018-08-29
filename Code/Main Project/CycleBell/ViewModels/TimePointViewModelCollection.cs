﻿using System;
using System.Collections.ObjectModel;

namespace CycleBell.ViewModels
{
    internal sealed class TimePointViewModelCollection : ObservableCollection<TimePointViewModelBase>
    {

        public TimePointViewModelCollection (PresetViewModel preset)
        {
            var timePoints = preset.TimePoints;

            if (timePoints == null)
                throw new ArgumentNullException (nameof(timePoints), @"Collection can't be null");

            if (timePoints.Count == 0)
                return;

            // TODO add to collection bounding TimePointViewModels:

        }
    }
}