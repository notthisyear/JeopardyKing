using System;
using System.Globalization;
using System.Windows.Data;

namespace JeopardyKing.WpfComponents.Converters
{
    public class StringShortenerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && parameter is string param && int.TryParse(param, NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxLength) && maxLength > 3)
                return (s.Length <= maxLength) ? s : $"{s[..(maxLength - 3)]}...";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
