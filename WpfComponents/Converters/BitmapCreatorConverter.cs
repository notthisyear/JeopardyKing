using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace JeopardyKing.WpfComponents.Converters
{
    public class BitmapCreatorConverter : IValueConverter
    {
        private BitmapImage? _emptyImage;
        private const string ResourceFolderName = "Resources";
        private const string EmptyImageName = "empty.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_emptyImage == default)
            {
                _emptyImage = new BitmapImage();
                _emptyImage.BeginInit();
                _emptyImage.UriSource = new(Path.Combine(AppContext.BaseDirectory, ResourceFolderName, EmptyImageName));
                _emptyImage.EndInit();
            }

            if (value is not string s || string.IsNullOrEmpty(s))
                return _emptyImage;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new(s);
            image.EndInit();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
