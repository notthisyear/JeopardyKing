using System;
using System.Globalization;
using System.Windows.Data;
using JeopardyKing.GameComponents;

namespace JeopardyKing.WpfComponents.Converters
{
    public class CalculateImageOrVideoHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return 0.0;

            if (values[0] is not Question q || values[1] is not double borderWidth || values[2] is not double borderHeight)
                return 0.0;

            if (q.Type != QuestionType.Image && q.Type != QuestionType.Video)
                return 0.0;

            if (IsWithinBoundingBox(q.ImageOrVideoWidth, q.ImageOrVideoHeight, (int)borderWidth, (int)borderHeight))
                return (double)q.ImageOrVideoHeight;

            var scaleFactor = Math.Min(borderWidth / q.ImageOrVideoWidth, borderHeight / q.ImageOrVideoHeight);
            return q.ImageOrVideoHeight * scaleFactor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static bool IsWithinBoundingBox(int w, int h, int boundingBoxWidth, int boundingBoxHeight)
            => (w <= boundingBoxWidth) && (h <= boundingBoxHeight);
    }
}
