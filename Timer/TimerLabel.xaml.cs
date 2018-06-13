using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Timer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TimerLabel : UserControl
    {
        public TimerLabel()
        {
            InitializeComponent();
        }

        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(TimerHourDecProperty); }
            set { SetValue(TimerHourDecProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimerHourDec.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimerHourDecProperty =
            DependencyProperty.Register("TimerHourDec", typeof(TimeSpan), typeof(TimerLabel), new PropertyMetadata(new TimeSpan(0, 0, 0)));


    }
}
