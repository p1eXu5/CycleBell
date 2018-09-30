using System;
using System.Collections.ObjectModel;
using System.Linq;
using CycleBell.ViewModels.TimePointViewModels;

namespace CycleBell.ViewModels
{
    /// <summary>
    /// Extension class for ReadOnlyObservableCollection&lt;TimePointViewModel&gt;
    /// </summary>
    internal static class TimePointViewModelExtension
    {
        internal static ReadOnlyObservableCollection<TimePointViewModelBase> Diactivate(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels)
        {
            if (timePointViewModels == null)
                return null;

            var tpvmArray = timePointViewModels.Where(t => (t is TimePointViewModel model) && model.Active).ToArray();

            foreach (var timePointViewModel in tpvmArray) {

                ((TimePointViewModel)timePointViewModel).Active = false;
            }

            return timePointViewModels;
        }

        internal static void Activate(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels, Func<TimePointViewModelBase,bool> predicate)
        {
            var tpvm = timePointViewModels?.Where(predicate).FirstOrDefault();

            if (tpvm == null)
                return;

            ((TimePointViewModel)tpvm).Active = true;
        }
    }
}
