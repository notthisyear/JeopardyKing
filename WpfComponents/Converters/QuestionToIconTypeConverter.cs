using System;
using System.Globalization;
using System.Windows.Data;
using JeopardyKing.GameComponents;

namespace JeopardyKing.WpfComponents.Converters
{
    public class QuestionToIconTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QuestionType type)
            {
                return type switch
                {
                    QuestionType.Text => IconType.Text,
                    QuestionType.Image => IconType.Image,
                    QuestionType.Audio => IconType.Audio,
                    QuestionType.Video => IconType.Video,
                    _ => IconType.None
                };
            }
            return IconType.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
