using System;
using System.Collections.ObjectModel;
using System.Linq;
using CycleBell.ViewModels.TimePointViewModels;

namespace CycleBell.ViewModels.TimePointViewModels
{
    /// <summary>
    /// Extension class for ReadOnlyObservableCollection&lt;TimePointViewModel&gt;
    /// </summary>
    internal static class TimePointViewModelExtension
    {
        internal static ReadOnlyObservableCollection<TimePointViewModelBase> DeactivateAll(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels)
        {
            if (timePointViewModels == null)
                return null;

            foreach (var timePointViewModel in timePointViewModels) {

                timePointViewModel.IsEnabled = false;
            }

            return timePointViewModels;
        }

        internal static ReadOnlyObservableCollection<TimePointViewModelBase> ActivateAll(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels)
        {
            if (timePointViewModels == null)
                return null;

            foreach (var timePointViewModel in timePointViewModels) {

                timePointViewModel.IsEnabled = true;
            }

            return timePointViewModels;
        }

        internal static void Activate(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels, Func<TimePointViewModelBase,bool> predicate)
        {
            var tpvm = timePointViewModels?.Where(predicate).FirstOrDefault();

            if (tpvm == null)
                return;

            ((TimePointViewModel)tpvm).IsEnabled = true;
        }
    }
}
