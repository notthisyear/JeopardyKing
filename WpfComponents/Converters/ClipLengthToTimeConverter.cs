using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JeopardyKing.WpfComponents.Converters
{
    internal class ClipLengthToTimeConverter : IMultiValueConverter
    {
        private readonly SecondsToTimeStringConverter _converter = new();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!values.Any() || values.Length != 2 || values[0] is not double start || values[1] is not double end)
                return "-";

            var length = end - start;
            return _converter.Convert(length, typeof(string), parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
