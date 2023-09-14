using System;
using System.Globalization;
using System.Windows.Data;

namespace JeopardyKing.WpfComponents.Converters
{
    public sealed class SecondsToTimeStringConverter : IValueConverter
    {
        private const int SecondsPerMinute = 60;
        private const int MinutesPerHour = 60;
        private const int SecondsPerHour = SecondsPerMinute * MinutesPerHour;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds;
            if (value is int i)
                seconds = (double)i;
            else if (value is double d)
                seconds = (double)d;
            else
                return 0;

            var numberOfHours = (int)(seconds / SecondsPerHour);
            seconds -= numberOfHours * SecondsPerHour;
            var numberOfMinutes = (int)(seconds / SecondsPerMinute);
            seconds -= numberOfMinutes * SecondsPerMinute;

            return $"{(numberOfHours > 0 ? $"{numberOfHours:D2}:" : "")}{numberOfMinutes:D2}:{(int)(seconds):D2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
