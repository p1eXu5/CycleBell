using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CycleBell.ViewModels;
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
