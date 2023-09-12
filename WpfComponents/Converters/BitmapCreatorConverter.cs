using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using JeopardyKing.GameComponents;

namespace JeopardyKing.WpfComponents.Converters
{
    public class BitmapCreatorConverter : IMultiValueConverter
    {
        private const string EmptyImageName = "emptyImage";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] is not Question q || q.Type != QuestionType.Image || string.IsNullOrEmpty(q.MultimediaContentLink))
                return (Application.Current.FindResource(EmptyImageName) as BitmapImage)!;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new(q.MultimediaContentLink);
            return image;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
