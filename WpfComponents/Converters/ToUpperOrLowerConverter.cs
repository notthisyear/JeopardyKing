using System;
using System.Globalization;
using System.Windows.Data;

namespace JeopardyKing.WpfComponents.Converters
{
    public class ToUpperOrLowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string s)
                return value;

            if (parameter is not string p)
                return value;

            if (p.Equals("lower", StringComparison.InvariantCultureIgnoreCase))
                return s.ToLower();
            else if (p.Equals("upper", StringComparison.InvariantCultureIgnoreCase))
                return s.ToUpper();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
