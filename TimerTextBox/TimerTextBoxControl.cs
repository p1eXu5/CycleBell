using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TimerTextBox
{
    // If class will have name match to app namespace than programm will have error
    public class TimerTextBoxControl : TextBox
    {
        private const byte DigitsFieldWidth = 5;

        private sbyte _engine = 1;
        private char[] _hoursDec = {' ', '0', '1', '2'};
        private char[] _hoursOneWhenPm = { '1', '2', '3' };
        private char[] _digits = {' ', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private char[] _minutesDec = {' ', '0', '1', '2', '3', '4', '5'};
        private bool _isSelectedState;
        private string _lastValue = null;
        private readonly string _defaultValue;

        public TimerTextBoxControl() : base()
        {
            _defaultValue = "00" + Separator + "00";
        }

        public char Separator { get; set; } = ':';

        protected override void OnPreviewTextInput (TextCompositionEventArgs e)
        {
            SolidColorBrush[] brushes = { Brushes.Aqua, Brushes.AliceBlue };
            byte index = 1;
            SetValue (BackgroundProperty, brushes[(index + _engine)/2]);
            _engine = (sbyte)-_engine;

            e.Handled = CheckTextInput(e.Text[0]);
        }

        // Возникает перед OnPreviewTextInput
        protected override void OnPreviewKeyDown (KeyEventArgs e)
        {
            //e.Handled = true;

            SetValue (TextProperty, $"{_engine}");

            base.OnPreviewKeyDown (e);
        }

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
    }
}
