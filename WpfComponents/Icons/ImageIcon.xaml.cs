using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JeopardyKing.WpfComponents
{
    public partial class ImageIcon : UserControl
    {
        public Brush FillColor
        {
            get { return (Brush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register(
            nameof(FillColor),
            typeof(Brush),
            typeof(ImageIcon),
            new FrameworkPropertyMetadata(new SolidColorBrush(new() { A = 150, R = 255, G = 255, B = 255 }),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush StrokeColor
        {
            get { return (Brush)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }
        public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register(
            nameof(StrokeColor),
            typeof(Brush),
            typeof(ImageIcon),
            new FrameworkPropertyMetadata(new SolidColorBrush(new() { A = 255, R = 0, G = 0, B = 0 }),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(ImageIcon),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double ScaleFactor
        {
            get { return (double)GetValue(ScaleFactorProperty); }
            set { SetValue(ScaleFactorProperty, value); }
        }
        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(
            nameof(ScaleFactor),
            typeof(double),
            typeof(ImageIcon),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange));

        public ImageIcon()
        {
            InitializeComponent();
        }
    }
}
