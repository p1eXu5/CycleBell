using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimerTextBox
{
    public class TimerLabelControl : Label
    {
        public TimerLabelControl() : base()
        {

        }

        public byte MaxNum
        {
            get { return (byte)GetValue(MaxNumProperty); }
            set { SetValue(MaxNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxNumProperty =
            DependencyProperty.Register("MaxNum", typeof(byte), typeof(TimerLabelControl), new PropertyMetadata((byte)9));
    }
}
