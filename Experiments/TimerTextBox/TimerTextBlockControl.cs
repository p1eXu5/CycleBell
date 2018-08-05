using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TimerTextBox
{
    public class TimerTextBlockControl : TextBlock
    {
        private bool _isValuedState;
        private Point _initPoint;

        public TimerTextBlockControl() : base()
        {

        }

        public byte MaxNum
        {
            get { return (byte)GetValue(MaxNumProperty); }
            set { SetValue(MaxNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxNumProperty =
            DependencyProperty.Register("MaxNum", typeof(byte), typeof(TimerTextBlockControl), new PropertyMetadata((byte)9));

        protected override void OnMouseDown (MouseButtonEventArgs e)
        {
            _isValuedState = true;
            _initPoint = PointToScreen(e.GetPosition (this));
            base.OnMouseDown (e);
        }

        protected override void OnMouseUp (MouseButtonEventArgs e)
        {
            _isValuedState = false;
            base.OnMouseUp (e);
        }

        protected override void OnMouseMove (MouseEventArgs e)
        {
            if (_isValuedState) {


            }
            
            base.OnMouseMove (e);
        }
    }
}
