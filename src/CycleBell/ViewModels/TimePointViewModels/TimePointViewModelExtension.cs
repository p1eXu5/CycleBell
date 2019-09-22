/*
 * Copyright © 2018 Vladimir Likhatskiy. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CycleBell.ViewModels.TimePointViewModels
{
    /// <summary>
    /// Extension class for ReadOnlyObservableCollection&lt;TimePointViewModel&gt;
    /// </summary>
    internal static class TimePointViewModelExtension
    {
        internal static ReadOnlyObservableCollection<TimePointViewModelBase> DisableAll(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels)
        {
            if (timePointViewModels == null)
                return null;

            foreach (var timePointViewModel in timePointViewModels) {

                timePointViewModel.IsEnabled = false;
            }

            return timePointViewModels;
        }

        internal static ReadOnlyObservableCollection<TimePointViewModelBase> EnableAll(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels)
        {
            if (timePointViewModels == null)
                return null;

            foreach (var timePointViewModel in timePointViewModels) {

                timePointViewModel.IsEnabled = true;
                timePointViewModel.IsActive = false;
            }

            return timePointViewModels;
        }

        internal static TimePointViewModelBase Activate(this ReadOnlyObservableCollection<TimePointViewModelBase> timePointViewModels, Func<TimePointViewModelBase,bool> predicate)
        {
            var tpvm = timePointViewModels?.Where(predicate).FirstOrDefault();

            if (tpvm == null)
                return null;

            tpvm.IsActive = true;
            return tpvm;
        }
    }
}
