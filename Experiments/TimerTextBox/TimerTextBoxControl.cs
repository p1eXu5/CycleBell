using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TimerTextBox
{
    class TimerTextBoxConverter : IMultiValueConverter
    {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append (values[0]);
            sb.Append (parameter);
            sb.Append (values[1]);

            return sb.ToString();
        }

        public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string input = (string)value;

            string[] res = new[]
            {
                input.Substring (0, 2), 
                input.Substring (2, 1),
                input.Substring (3, 2)
            };

            return new [] {DependencyProperty.UnsetValue, DependencyProperty.UnsetValue};
        }
    }

    // If class will have name match to app namespace than programm will have error
    public class TimerTextBoxControl : TextBox
    {
        #region private section
        private static TimerTextBoxConverter _converter = new TimerTextBoxConverter();

        private const byte DigitsFieldWidth = 5;

        private sbyte _engine = 1;
        private char[] _hoursDec = {' ', '0', '1', '2'};
        private char[] _hoursOneWhenPm = { '1', '2', '3' };
        private char[] _digits = {' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private char[] _minutesDec = {' ', '0', '1', '2', '3', '4', '5'};
        private bool _isSelectedState;
        private string _lastValue = null;
        private readonly string _defaultValue;
        private bool _raised = false;

        #endregion

        #region Constructor

        public TimerTextBoxControl() : base()
        {
            _defaultValue = "00" + Separator + "00";

            MultiBinding mb = new MultiBinding();

            mb.Bindings.Add(new Binding {Path = new PropertyPath ("Hours"), Source = new RelativeSource{Mode=RelativeSourceMode.Self}});
            mb.Bindings.Add(new Binding {Path = new PropertyPath ("Minutes"), Source=new RelativeSource{Mode = RelativeSourceMode.Self}});
            mb.Converter = _converter;
            mb.ConverterParameter = Separator;

            SetBinding(TextBox.TextProperty, mb);
        }

        #endregion Constructor

        #region Dependency Properties

        public string Hours
        {
            get { return (string)GetValue(HoursProperty); }
            set { SetValue(HoursProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hours.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register("Hours", typeof(string), typeof(TimerTextBoxControl), new PropertyMetadata("00"));



        public string Minutes
        {
            get { return (string)GetValue(MinutesProperty); }
            set { SetValue(MinutesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minutes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(string), typeof(TimerTextBoxControl), new PropertyMetadata("00"));



        #endregion Dependency Properties

        #region CLR Properties

        public char Separator { get; set; } = ':';

        #endregion

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            SolidColorBrush[] brushes = { Brushes.Aqua, Brushes.AliceBlue };
            byte index = 1;
            SetValue(BackgroundProperty, brushes[(index + _engine) / 2]);
            _engine = (sbyte)-_engine;

            //e.Handled = CheckTextInput(e.Text[0]);

        }

        // Возникает перед OnPreviewTextInput
        protected override void OnPreviewKeyDown (KeyEventArgs e)
        {
            //e.Handled = true;

            //SetValue (TextProperty, $"{_engine}");

            if (e.Key == Key.Enter) {
                _raised = e.Handled = true;
                RoutedEventArgs args = new RoutedEventArgs(TimeEditIsDoneEvent);
                RaiseEvent (args);
            }
            //base.OnPreviewKeyDown (e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (!_raised) {
                RoutedEventArgs args = new RoutedEventArgs(TimeEditIsDoneEvent);
                RaiseEvent(args);
            }

            _raised = false;

            base.OnLostFocus(e);
        }

        /// <summary>
        /// CheckTextInput
        /// </summary>
        /// <param name="inputChar"></param>
        /// <returns></returns>
        protected bool CheckTextInput (char inputChar)
        {
            if (SelectionLength > 0)
                _isSelectedState = true;

            // Условия, при которых _isSelectionState станет false:
            //
            //  - Элемент потеряет фокус
            //  - При клике мышки позиция изменится
            //  - Наборщик будет в крайней позиции
            // TODO:

            if (Text.Length > DigitsFieldWidth) {

                _lastValue = Text;
                Text = _defaultValue;

                return true;
            }

            if (inputChar == ' ') {
                inputChar = '0';
            }
            else {
                bool res = false;

                switch (SelectionStart) {
                    case 0:
                        res = _hoursDec.Contains (inputChar);
                        break;
                    case 1:
                    case 4:
                        res = _digits.Contains (inputChar);
                        break;
                    case 3:
                        res = _minutesDec.Contains (inputChar);
                        break;
                }

                // Если не попали, то Handled = true, дальше символ не пускаем
                if (res == false) return true;
            }


            if (SelectionStart == 0) {

                if ((Text[SelectionStart] != inputChar) && (inputChar == '2') && !(_hoursOneWhenPm.Contains (Text[1]))) {

                    // Десятки часа изменились, единицы часа в диапазоне от 4 до 9
                    _lastValue = Text.Substring (0, 2);
                    Text = Text[0] + '0' + Text.Substring (2);

                    return true;
                } 
            }

            return false;
        }

        public event RoutedEventHandler TimeEditIsDone
        {
            add { AddHandler (TimeEditIsDoneEvent, value); }
            remove { RemoveHandler (TimeEditIsDoneEvent, value);}
        }

        public static readonly RoutedEvent TimeEditIsDoneEvent = EventManager.RegisterRoutedEvent ("TimeEditIsDone", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimerTextBoxControl));
    }
}
