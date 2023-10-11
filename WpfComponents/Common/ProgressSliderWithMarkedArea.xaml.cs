using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using JeopardyKing.WpfComponents.CustomEventArgs;

namespace JeopardyKing.WpfComponents
{
    public partial class ProgressSliderWithMarkedArea : UserControl
    {
        #region Dependency properties
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(0.0));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(1.0));

        public double ProgressMarkerValue
        {
            get => (double)GetValue(ProgressMarkerValueProperty);
            set => SetValue(ProgressMarkerValueProperty, value);
        }
        public static readonly DependencyProperty ProgressMarkerValueProperty = DependencyProperty.Register(
            nameof(ProgressMarkerValue),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, default, ClampToFullRange));

        public double AreaStart
        {
            get => (double)GetValue(AreaStartProperty);
            set => SetValue(AreaStartProperty, value);
        }
        public static readonly DependencyProperty AreaStartProperty = DependencyProperty.Register(
            nameof(AreaStart),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AreaStartChangedCallback, ClampLowerToRange));

        public double AreaEnd
        {
            get => (double)GetValue(AreaEndProperty);
            set => SetValue(AreaEndProperty, value);
        }
        public static readonly DependencyProperty AreaEndProperty = DependencyProperty.Register(
            nameof(AreaEnd),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(0.8, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AreaEndChangedCallback, ClampUpperToRange));

        public SolidColorBrush MainBackgroundColor
        {
            get => (SolidColorBrush)GetValue(MainBackgroundColorProperty);
            set => SetValue(MainBackgroundColorProperty, value);
        }
        public static readonly DependencyProperty MainBackgroundColorProperty = DependencyProperty.Register(
            nameof(MainBackgroundColor),
            typeof(SolidColorBrush),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(new SolidColorBrush(new Color() { R = 255, G = 0, B = 0 }), FrameworkPropertyMetadataOptions.AffectsRender));

        public SolidColorBrush MarkedAreaColor
        {
            get => (SolidColorBrush)GetValue(MarkedAreaColorProperty);
            set => SetValue(MarkedAreaColorProperty, value);
        }
        public static readonly DependencyProperty MarkedAreaColorProperty = DependencyProperty.Register(
            nameof(MarkedAreaColor),
            typeof(SolidColorBrush),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(new SolidColorBrush(new Color() { R = 0, G = 255, B = 0 }), FrameworkPropertyMetadataOptions.AffectsRender));

        #region Read-only dependency properties
        public double MarkedAreaCanvasLeft
        {
            get => (double)GetValue(s_markedAreaCanvasLeftProperty);
            private set => SetValue(s_markedAreaCanvasLeftPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_markedAreaCanvasLeftPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(MarkedAreaCanvasLeft),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_markedAreaCanvasLeftProperty = s_markedAreaCanvasLeftPropertyKey.DependencyProperty;

        public double MarkedAreaWidth
        {
            get => (double)GetValue(s_markedAreaWidthProperty);
            private set => SetValue(s_markedAreaWidthPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_markedAreaWidthPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(MarkedAreaWidth),
            typeof(double),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_markedAreaWidthProperty = s_markedAreaWidthPropertyKey.DependencyProperty;

        public bool DragActive
        {
            get => (bool)GetValue(s_dragActiveProperty);
            private set => SetValue(s_dragActivePropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_dragActivePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(DragActive),
            typeof(bool),
            typeof(ProgressSliderWithMarkedArea),
            new FrameworkPropertyMetadata(false));
        private static readonly DependencyProperty s_dragActiveProperty = s_dragActivePropertyKey.DependencyProperty;
        #endregion

        #endregion

        #region Property changed and coerce callbacks
        private static object ClampToFullRange(DependencyObject d, object baseValue)
        {
            if (d is ProgressSliderWithMarkedArea p && baseValue is double val)
                return Math.Clamp(val, p.Minimum, p.Maximum);
            return baseValue;
        }

        private static object ClampUpperToRange(DependencyObject d, object baseValue)
        {
            if (d is ProgressSliderWithMarkedArea p && baseValue is double val)
                return Math.Clamp(val, p.AreaStart >= p.Maximum ? p.Minimum : p.AreaStart, p.Maximum);
            return baseValue;
        }

        private static object ClampLowerToRange(DependencyObject d, object baseValue)
        {
            if (d is ProgressSliderWithMarkedArea p && baseValue is double val)
                return Math.Clamp(val, p.Minimum, p.AreaEnd <= p.Minimum ? p.Maximum : p.AreaEnd);
            return baseValue;
        }
        private static void AreaStartChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ProgressSliderWithMarkedArea p || e.NewValue is not double val)
                return;

            if (val > p.AreaEnd)
                return;

            var range = p.Maximum - p.Minimum;
            if (range > 0)
            {
                p.MarkedAreaWidth = (p.AreaEnd - val) / (range) * p.Width;
                p.MarkedAreaCanvasLeft = p.Width * val / (range);
            }
        }

        private static void AreaEndChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ProgressSliderWithMarkedArea p || e.NewValue is not double val)
                return;

            if (val < p.AreaStart)
                return;

            var range = p.Maximum - p.Minimum;
            if (range > 0)
            {
                p.MarkedAreaWidth = (val - p.AreaStart) / (range) * p.Width;
                p.MarkedAreaCanvasLeft = p.Width * p.AreaStart / (range);
            }
        }

        #endregion

        public event RoutedEventHandler ProgressMarkerDragDone
        {
            add { AddHandler(ProgressMarkerDragDoneEvent, value); }
            remove { RemoveHandler(ProgressMarkerDragDoneEvent, value); }
        }
        public static readonly RoutedEvent ProgressMarkerDragDoneEvent = EventManager.RegisterRoutedEvent(
            nameof(ProgressMarkerDragDone),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ProgressSliderWithMarkedArea));

        public ProgressSliderWithMarkedArea()
        {
            InitializeComponent();
        }

        private void ThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            DragActive = true;
        }

        private void ThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            DragActive = false;
            if (sender is not Slider s)
                return;

            RaiseEvent(new ProgressMarkerDragDoneEventArgs(ProgressMarkerDragDoneEvent, s.Value));
        }
    }
}
