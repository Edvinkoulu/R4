using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Village_Newbies.Converters
{
    public class ZeroToEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is decimal decimalValue && decimalValue == 0)
                return string.Empty;

            if (value is double doubleValue && doubleValue == 0)
                return string.Empty;

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrWhiteSpace(str))
                return 0;

            if (targetType == typeof(decimal) && decimal.TryParse(str, out var decimalResult))
                return decimalResult;

            if (targetType == typeof(double) && double.TryParse(str, out var doubleResult))
                return doubleResult;

            return 0;
        }
    }
}