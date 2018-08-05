using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CycleBell.Controls
{
    public class TextBox2 : TextBox
    {
        private const sbyte MinutesTensLimit = 5;
        private const sbyte HoursTensLimit = 2;
        private const sbyte HoursOnesLimit = 3;

        private bool nextOnes = false;

        public TextBox2() : base()
        {
        }

        /// <summary>
        /// TextBox2 OnPreviewKeyDown handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {

                e.Handled = true;
                this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            else if (e.Key == Key.Back) {

                e.Handled = true;
                
                if (Text.Length == 5) {
                    switch (CaretIndex) {
                        // 1|0:00
                        case 1:
                            Text = Text.Substring(1);
                            CaretIndex = 0;
                            break;
                        // 01|:00, 01:|00
                        case 2:
                        case 3:
                            Text = Text[0] + "0" + Text.Substring(2);
                            CaretIndex = 1;
                            break;
                        // 00:1|0
                        case 4:
                            Text = Text.Substring(0, 3) + "0" + Text[4];
                            CaretIndex = 3;
                            break;
                        // 00:01|
                        case 5:
                            Text = Text.Substring(0, 4) + "0";
                            CaretIndex = 4;
                            break;
                    }
                }
                else {
                    switch (CaretIndex) {
                        // 1|:00, 1:|00
                        case 1:
                        case 2:
                            Text = "0" + Text.Substring(1);
                            CaretIndex = 0;
                            break;
                        // 0:1|0
                        case 3:
                            Text = Text.Substring(0, 2) + "0" + Text[3];
                            CaretIndex = 2;
                            break;
                        // 0:01|
                        case 4:
                            Text = Text.Substring(0, 3) + "0";
                            CaretIndex = 3;
                            break;
                    }
                }
            }

            if (e.Key == Key.Delete)
                e.Handled = true;

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// TextBox2 OnPreviewTextInput handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            Debug.WriteLine(this.CaretIndex);

            int currentIndex = CaretIndex;
            
            if (CaretIndex == Text.Length)
                goto Handled;

            sbyte value = (sbyte)Char.GetNumericValue(e.Text[0]);

            if (value < 0)
                goto Handled;

            switch (Text.Length) {
                case 5:
                    switch (CaretIndex) {
                        // 10:0|0
                        case 4:
                            SetMinuteNum(value);
                            break;
                        // 10:|00
                        case 3:
                            SetMinuteDecNum(value);
                            break;
                        // 10|:00
                        case 2:
                            ++CaretIndex;
                            if (!SetMinuteDecNum(value)) {
                                --CaretIndex;
                            }
                            break;
                        // 1|0:00
                        case 1:
                            SetHourNum5(value);
                            break;
                        // |10:00
                        case 0:
                            SetHourDecNum5(value);
                            break;
                    }
                    break;
                case 4:
                    switch (CaretIndex) {
                        // 0:0|0
                        case 3:
                            SetMinuteNum(value);
                            break;
                        // 0:|00
                        case 2:
                            SetMinuteDecNum(value);
                            break;
                        // 0|:00
                        case 1:
                            ++CaretIndex;
                            if (!SetMinuteDecNum(value)) {
                                --CaretIndex;
                            }
                            break;
                        // |0:00
                        case 0:
                            SetHourDecNum4(value);
                            break;
                    }
                    break;
            }

        Handled:
            e.Handled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMinuteNum(sbyte num)
        {
            int caret = CaretIndex;
            Text = Text.Substring(0, CaretIndex) + num;
            CaretIndex = caret + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SetMinuteDecNum(sbyte num)
        {
            if (num <= MinutesTensLimit) {

                int caret = CaretIndex;
                Text = Text.Substring(0, CaretIndex) + num + Text.Substring(CaretIndex + 1);
                CaretIndex = caret + 1;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetHourNum5(sbyte num)
        {
            if (Text[0] == '1') {
                Text = "" + Text[0] + num + Text.Substring(2);
                CaretIndex = 3;
            }
            else {
                // if Text[0] == '2'
                if (num <= HoursOnesLimit) {
                    Text = "" + Text[0] + num + Text.Substring(2);
                    CaretIndex = 3;
                }
            }

        }

        /// <summary>
        /// Установка десятков часов, когда часы меньше десяти
        /// </summary>
        /// <param name="num"></param>
        private void SetHourDecNum4(sbyte num)
        {
            if (!nextOnes && num <= HoursTensLimit) {

                if (num == 0) {
                    nextOnes = true;
                    return;
                }

                Text = "" + num + Text;
                CaretIndex = 1;
            }
            else if (nextOnes) {

                nextOnes = false;
                Text = "" + num + Text.Substring(1);
                CaretIndex = 2;
            }

        }

        private void SetHourDecNum5(sbyte num)
        {
            switch (num) {
                case 0:
                    Text = Text.Substring(1);
                    CaretIndex = 0;
                    break;
                case 1:
                case 2:
                    Text = "" + num + Text.Substring(1);
                    CaretIndex = 1;
                    break;
            }

        }

        /// <summary>
        /// OnLostFocus handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Debug.WriteLine("Lost focus");

            if (Text.Length == 5) {

                if (Text[0] == '2') {

                    if (Char.GetNumericValue(Text[1]) >= 4.0) {
                        Text = "23" + Text.Substring(2);
                    }
                }
            }

            base.OnLostFocus(e);
        }

        
    }
}
