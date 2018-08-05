using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan)) {
                throw new ArgumentException("Value must be TimeSpan", nameof(value));
            };

            TimeSpan ts = (TimeSpan)value;

            if (ts.Ticks < 0) {
                ts = ts.Negate();
            }

            StringBuilder sb = new StringBuilder();

            sb.Append((int)ts.TotalHours);
            sb.Append(':');

            if (ts.Minutes <= 9) {
                sb.Append('0');
            }
            sb.Append(ts.Minutes);
            sb.Append(':');

            if (ts.Seconds <= 9) {
                sb.Append('0');
            }
            sb.Append(ts.Seconds);
            sb.Append('.');

            if (ts.Milliseconds <= 99) {
                sb.Append('0');
            }
            if (ts.Milliseconds <= 9) {
                sb.Append('0');
            }
            sb.Append(ts.Milliseconds);

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
