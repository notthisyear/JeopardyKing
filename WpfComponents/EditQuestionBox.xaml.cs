using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using JeopardyKing.ViewModels;
using JeopardyKing.Windows;

namespace JeopardyKing.WpfComponents
{
    public partial class EditQuestionBox : UserControl
    {
        #region Dependency properties
        public EditQuestionBoxViewModel ViewModel
        {
            get => (EditQuestionBoxViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(EditQuestionBoxViewModel),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(default));

        public double EditQuestionOpacityValue
        {
            get => (double)GetValue(EditQuestionOpacityValueProperty);
            set => SetValue(EditQuestionOpacityValueProperty, value);
        }
        public static readonly DependencyProperty EditQuestionOpacityValueProperty = DependencyProperty.Register(
            nameof(EditQuestionOpacityValue),
            typeof(double),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double EditQuestionBoxXValue
        {
            get => (double)GetValue(EditQuestionBoxXValueProperty);
            set => SetValue(EditQuestionBoxXValueProperty, value);
        }
        public static readonly DependencyProperty EditQuestionBoxXValueProperty = DependencyProperty.Register(
            nameof(EditQuestionBoxXValue),
            typeof(double),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ValueIsBeingEdited
        {
            get => (bool)GetValue(ValueIsBeingEditedProperty);
            set => SetValue(ValueIsBeingEditedProperty, value);
        }
        public static readonly DependencyProperty ValueIsBeingEditedProperty = DependencyProperty.Register(
            nameof(ValueIsBeingEdited),
            typeof(bool),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Private fields
        private bool _editQuestionBoxIsToTheLeft;
        #endregion

        public EditQuestionBox()
        {
            InitializeComponent();
            Loaded += EditQuestionBoxLoaded;
        }

        private void EditQuestionBoxLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EditQuestionBoxLoaded;
            _editQuestionBoxIsToTheLeft = EditQuestionBoxShouldBeToTheLeft(Application.Current.MainWindow.ActualWidth);

            // FIXME: This is a hack to move the edit box to the right initially
            //        as the mouse is "typically" to the left in the beginning.
            //        We should come up with a better solution to ensure that the
            //        edit question box is in the correct place.
            BeginAnimation(EditQuestionBoxXValueProperty,
                        GetEditQuestionXValueAnimation(
                            GetEditQuestionBoxRight(
                                Application.Current.MainWindow.ActualWidth,
                                editQuestionBox.ActualWidth,
                                editQuestionBox.Margin), 0.0));
            ViewModel.ModeManager.PropertyChanged += ModeManagerPropertyChanged;
        }

        private void ModeManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CreateWindowModeManager modeManager)
                return;

            if (e.PropertyName == nameof(modeManager.CurrentState))
            {
                if (modeManager.CurrentState == CreateWindowState.NothingSelected)
                {
                    BeginAnimation(EditQuestionOpacityValueProperty, GetEditQuestionOpacityAnimation(0.0, EditQuestionOpacityValue));
                    return;
                }

                if (modeManager.CurrentState == CreateWindowState.QuestionHighlighted)
                {
                    BeginAnimation(EditQuestionOpacityValueProperty, GetEditQuestionOpacityAnimation(0.95, EditQuestionOpacityValue));
                    var shouldBeToTheLeft = EditQuestionBoxShouldBeToTheLeft(Application.Current.MainWindow.ActualWidth);
                    if (_editQuestionBoxIsToTheLeft == shouldBeToTheLeft)
                        return;

                    var leftPosition = GetEditQuestionBoxLeft(editQuestionBox.Margin);
                    var rightPosition = GetEditQuestionBoxRight(Application.Current.MainWindow.ActualWidth, editQuestionBox.ActualWidth, editQuestionBox.Margin);

                    BeginAnimation(EditQuestionBoxXValueProperty,
                        GetEditQuestionXValueAnimation(
                            shouldBeToTheLeft ? leftPosition : rightPosition,
                            shouldBeToTheLeft ? rightPosition : leftPosition));

                    _editQuestionBoxIsToTheLeft = shouldBeToTheLeft;
                }
            }
        }

        private void EditValueButtonClicked(object sender, RoutedEventArgs e)
        {
            ValueIsBeingEdited = true;
            editValueBox.Focus();
            editValueBox.CaretIndex = editValueBox.Text.Length;
        }

        private void KeyPressedEditBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
                ValueIsBeingEdited = false;
        }

        #region Static methods
        private static DoubleAnimation GetEditQuestionOpacityAnimation(double to, double from)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.2)
            };
        }

        private static DoubleAnimation GetEditQuestionXValueAnimation(double to, double from)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 5
                }
            };
        }

        private static bool EditQuestionBoxShouldBeToTheLeft(double windowWidth)
            => Mouse.GetPosition(Application.Current.MainWindow).X > windowWidth / 2;

        private static double GetEditQuestionBoxRight(double windowWidth, double boxWidth, Thickness boxMargin)
            => windowWidth - boxWidth - (3 * boxMargin.Right);

        private static double GetEditQuestionBoxLeft(Thickness boxMargin)
                => boxMargin.Left;
        #endregion
    }
}
