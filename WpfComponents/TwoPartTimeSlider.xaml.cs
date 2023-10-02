using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JeopardyKing.WpfComponents
{
    public partial class TwoPartTimeSlider : UserControl
    {
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(TwoPartTimeSlider),
            new FrameworkPropertyMetadata(0.0));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(TwoPartTimeSlider),
            new FrameworkPropertyMetadata(0.0));

        public double LowerValue
        {
            get => (double)GetValue(LowerValueProperty);
            set => SetValue(LowerValueProperty, value);
        }
        public static readonly DependencyProperty LowerValueProperty = DependencyProperty.Register(
            nameof(LowerValue),
            typeof(double),
            typeof(TwoPartTimeSlider),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, LowerValueChangedCallback, LowerValueCoerceCallback));

        public double UpperValue
        {
            get => (double)GetValue(UpperValueProperty);
            set => SetValue(UpperValueProperty, value);
        }
        public static readonly DependencyProperty UpperValueProperty = DependencyProperty.Register(
            nameof(UpperValue),
            typeof(double),
            typeof(TwoPartTimeSlider),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, UpperValueChangedCallback, UpperValueCoerceCallback));

        public double Difference
        {
            get => (double)GetValue(s_differenceProperty);
            private set => SetValue(s_differencePropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_differencePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Difference),
            typeof(double),
            typeof(TwoPartTimeSlider),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_differenceProperty = s_differencePropertyKey.DependencyProperty;

        public bool IsMouseOverComponent
        {
            get => (bool)GetValue(s_isMouseOverComponentProperty);
            private set => SetValue(s_isMouseOverComponentPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_isMouseOverComponentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsMouseOverComponent),
            typeof(bool),
            typeof(TwoPartTimeSlider),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_isMouseOverComponentProperty = s_isMouseOverComponentPropertyKey.DependencyProperty;

        private static void LowerValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TwoPartTimeSlider slider && e.NewValue is double v)
                slider.Difference = slider.UpperValue - v;
        }

        private static void UpperValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TwoPartTimeSlider slider && e.NewValue is double v)
                slider.Difference = v - slider.LowerValue;

        }
        private static object LowerValueCoerceCallback(DependencyObject d, object baseValue)
        {
            if (d is not TwoPartTimeSlider target || baseValue is not double val)
                return baseValue;
            return Math.Min(val, target.UpperValue <= 0 ? target.Maximum : target.UpperValue);
        }

        private static object UpperValueCoerceCallback(DependencyObject d, object baseValue)
        {
            if (d is not TwoPartTimeSlider target || baseValue is not double val)
                return baseValue;
            return Math.Max(val, target.LowerValue);
        }

        public TwoPartTimeSlider()
        {
            InitializeComponent();
        }

        private void ThumbMouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseOverComponent = true;
        }

        private void ThumbMouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOverComponent = false;
        }

        private void MouseClickedInComponent(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                LowerValue = Minimum;
                UpperValue = Maximum;
            }
        }
    }
}
