using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CycleBell.Converters
{
    public class FromTimeSpanToStringConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpanValue) {

                return timeSpanValue.ToString (@"h\:mm");
            }

            return null;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = (string)value;

            if (String.IsNullOrWhiteSpace (val))
                return TimeSpan.Zero;

            var res = TimeSpan.Parse (val);
            return res;
        }
    }
}
