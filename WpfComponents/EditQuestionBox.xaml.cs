using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using JeopardyKing.Common;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.WpfComponents
{
    public partial class EditQuestionBox : UserControl
    {
        #region Dependency properties
        public CreateWindowModeManager ModeManager
        {
            get => (CreateWindowModeManager)GetValue(ModeManagerProperty);
            set => SetValue(ModeManagerProperty, value);
        }
        public static readonly DependencyProperty ModeManagerProperty = DependencyProperty.Register(
            nameof(ModeManager),
            typeof(CreateWindowModeManager),
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

        public List<string>? CurrencyNames
        {
            get => (List<string>)GetValue(s_currencyNames);
            private set => SetValue(s_currencyNamesKey, value);
        }
        private static readonly DependencyPropertyKey s_currencyNamesKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrencyNames),
            typeof(List<string>),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_currencyNames = s_currencyNamesKey.DependencyProperty;

        public string SelectedCurrency
        {
            get => (string)GetValue(SelectedCurrencyProperty);
            set => SetValue(SelectedCurrencyProperty, value);
        }
        public static readonly DependencyProperty SelectedCurrencyProperty = DependencyProperty.Register(
            nameof(SelectedCurrency),
            typeof(string),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(string.Empty, SelectedCurrencyChanged));

        private static void SelectedCurrencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditQuestionBox editBox && e.NewValue is string newName)
            {
                if (editBox.ModeManager != default && editBox.ModeManager.CurrentlySelectedQuestion != default)
                    editBox.ModeManager.CurrentlySelectedQuestion.Currency = editBox._currencyNameMap[newName];
            }
        }
        #endregion

        #region Custom events
        public event RoutedEventHandler QuestionDeletionRequest
        {
            add { AddHandler(QuestionDeletionRequestEvent, value); }
            remove { RemoveHandler(QuestionDeletionRequestEvent, value); }
        }
        public static readonly RoutedEvent QuestionDeletionRequestEvent = EventManager.RegisterRoutedEvent(
            nameof(QuestionDeletionRequest),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(EditQuestionBox));

        #endregion

        #region Private fields
        private bool _editQuestionBoxIsToTheLeft;
        private readonly Dictionary<string, CurrencyType> _currencyNameMap;
        private readonly Dictionary<CurrencyType, string> _currencyTypeMap;
        private Regex r = ExtractVideoIdRegex();
        #endregion

        public EditQuestionBox()
        {
            CurrencyNames = new();
            _currencyNameMap = new();
            _currencyTypeMap = new();

            SetupCurrencyNameMaps();

            SelectedCurrency = CurrencyNames.First();

            InitializeComponent();
            Loaded += EditQuestionBoxLoaded;
        }

        private void SetupCurrencyNameMaps()
        {
            foreach (var t in Enum.GetValues<CurrencyType>())
            {
                var (attr, _) = t.GetCustomAttributeFromEnum<CurrencyAttribute>();
                if (attr != default)
                {
                    var displayName = $"{attr.Name} ({attr.Code})";
                    CurrencyNames!.Add(displayName);
                    _currencyNameMap.Add(displayName, t);
                    _currencyTypeMap.Add(t, displayName);
                }
            }
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
            ModeManager.PropertyChanged += ModeManagerPropertyChanged;
        }

        private void ModeManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CreateWindowModeManager modeManager)
                return;

            if (e.PropertyName == nameof(modeManager.CurrentlySelectedQuestion))
            {
                if (modeManager.CurrentlySelectedQuestion != null)
                    SelectedCurrency = _currencyTypeMap[modeManager.CurrentlySelectedQuestion.Currency];
            }
            else if (e.PropertyName == nameof(modeManager.CurrentState))
            {
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
                else if (modeManager.CurrentState == CreateWindowState.NothingSelected)
                {
                    BeginAnimation(EditQuestionOpacityValueProperty, GetEditQuestionOpacityAnimation(0.0, EditQuestionOpacityValue));
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
                CloseEditBox(sender, e);
        }

        private void CloseEditBox(object sender, RoutedEventArgs e)
        {
            ValueIsBeingEdited = false;
        }

        private void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            PopupWindowModal confirmationDialog = new(Application.Current.MainWindow, "Are you sure?", "Delete question", x =>
            {
                if (x == ModalWindowButton.OK)
                {
                    if (ModeManager.CurrentlySelectedQuestion == default)
                        return;

                    RaiseEvent(new(QuestionDeletionRequestEvent));
                    ModeManager.CurrentlySelectedQuestion = null;
                    ModeManager.CurrentState = CreateWindowState.NothingSelected;
                }
            },
           $"You are about to delete a question. Are you sure?");
            _ = confirmationDialog.ShowDialog();
        }

        private void LoadMediaButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ModeManager.CurrentlySelectedQuestion == default)
                return;

            VistaOpenFileDialog dialog = new()
            {
                Title = $"Select {ModeManager.CurrentlySelectedQuestion.Type} resource",
                Multiselect = false,
                Filter = GetFileExtensionsForType(ModeManager.CurrentlySelectedQuestion.Type),
            };

            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
            }
        }

        private void AddYouTubeLinkButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ModeManager.CurrentlySelectedQuestion == default)
                return;

            PopupWindowModal confirmationDialog = new(Application.Current.MainWindow, "Add YouTube link", string.Empty, (response, link) =>
            {
                if (response == ModalWindowButton.OK && TryExtractVideoId(link, out var videoId))
                {
                    ModeManager.CurrentlySelectedQuestion.IsEmbeddedMedia = false;
                    ModeManager.CurrentlySelectedQuestion.YouTubeVideoId = link;
                    ModeManager.CurrentlySelectedQuestion.MultimediaContentLink = videoId;
                }
            }, ModeManager.CurrentlySelectedQuestion.IsEmbeddedMedia ?
            string.Empty : ModeManager.CurrentlySelectedQuestion.YouTubeVideoId,
            ValidateYouTubeLink);
            _ = confirmationDialog.ShowDialog();
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

        private static string GetFileExtensionsForType(QuestionType type)
            => type switch
            {
                QuestionType.Image => "PNG images (*.png)|*.png|JPG images (*.jpg)|*.jpg|Bitmap images (*.bmp)|*.bmp",
                QuestionType.Audio => "All files (*.*)|*.*",
                QuestionType.Video => "All files (*.*)|*.*",
                _ => throw new NotSupportedException(),
            };

        private static (bool isValid, string errorMessage) ValidateYouTubeLink(string link)
        {
            if (string.IsNullOrEmpty(link))
                return (false, "YouTube link invalid - input is empty");

            var linkValid = TryExtractVideoId(link, out _);
            return (linkValid, linkValid ? string.Empty : "YouTube link invalid - could not extract video ID");
        }

        private static bool TryExtractVideoId(string link, out string videoId)
        {
            var m = ExtractVideoIdRegex().Match(link);
            videoId = m.Success ? m.Value : string.Empty;
            return m.Success;
        }

        [GeneratedRegex(@"((?<=(v|V)/)|(?<=(\\?|\\&)v=)|(?<=be/)|(?<=embed/)|(?<=shorts/))([a-zA-Z0-9-_]{5,30})")]
        private static partial Regex ExtractVideoIdRegex();
        #endregion
    }
}
