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

using System.Windows;
using System.Windows.Controls;
using CycleBell.ViewModels.TimePointViewModels;

namespace CycleBell.DataTemplateSelectors
{
    public class TimePointVmDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate (object item, DependencyObject container)
        {
            if (container is FrameworkElement element) {
                if (item is BeginTimePointViewModel) {

                    return element.FindResource ("datatempl_BeginTimePointViewModel") as DataTemplate;
                }
                if (item is EndTimePointViewModel) {

                    return element.FindResource ("datatempl_EndTimePointViewModel") as DataTemplate;
                }
                if (item is TimePointViewModel) {

                    return element.FindResource ("datatempl_TimePointViewModel") as DataTemplate;
                }

            }

            return null;
        }
    }
}
