using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using JeopardyKing.GameComponents;
using JeopardyKing.ViewModels;
using JeopardyKing.Windows;
using LibVLCSharp.Shared;

namespace JeopardyKing.WpfComponents
{
    using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

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
        private readonly LibVLC _libVlc;
        private static readonly Dictionary<(double to, double from), DoubleAnimation> s_opacityAnimationCache = new();
        #endregion

        public EditQuestionBox()
        {
            InitializeComponent();
            _libVlc = new LibVLC();
            Loaded += EditQuestionBoxLoaded;
        }

        private void EditQuestionBoxLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EditQuestionBoxLoaded;

            // Note: In Designer mode, Application.Current.MainWindow is not set, so the designer crashes
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
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

                if (ViewModel != default)
                {
                    ViewModel.ModeManager.PropertyChanged += ModeManagerPropertyChanged;
                    ViewModel.MultimediaParametersChanged += MultimediaParametersChanged;
                }

                playPauseIcon.IconType = IconType.Play;
                var mediaPlayer = new VlcMediaPlayer(_libVlc);
                audioVideoPlayer.MediaPlayer = mediaPlayer;
            }
        }

        private void MultimediaParametersChanged(object? sender, EventArgs e)
        {
            if (ViewModel == default || ViewModel.ModeManager.CurrentlySelectedQuestion == default)
                return;

            var mediaPlayer = audioVideoPlayer.MediaPlayer!;
            var isAudioOrVideo = ViewModel.ModeManager.CurrentlySelectedQuestion.Type == QuestionType.Video ||
                                 ViewModel.ModeManager.CurrentlySelectedQuestion.Type == QuestionType.Audio;
            var hasMultimediaLink = !string.IsNullOrEmpty(ViewModel.ModeManager.CurrentlySelectedQuestion.MultimediaContentLink);
            var hadMedia = mediaPlayer.Media != default;

            if (hadMedia && !hasMultimediaLink)
            {
                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Stop();
                    playPauseIcon.IconType = IconType.Play;
                }
                mediaPlayer.Media = default;
                playPauseIcon.Visibility = Visibility.Collapsed;
            }
            else if (isAudioOrVideo && hasMultimediaLink)
            {
                if (ViewModel.ModeManager.CurrentlySelectedQuestion.IsYoutubeLink == false)
                {
                    using var media = new Media(_libVlc, ViewModel.ModeManager.CurrentlySelectedQuestion.MultimediaContentLink);
                    mediaPlayer.Media = media;
                    playPauseIcon.Visibility = Visibility.Visible;
                }
            }
        }

        private void ModeManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CreateWindowModeManager modeManager)
                return;

            if (e.PropertyName == nameof(modeManager.CurrentState))
            {
                if (modeManager.CurrentState == CreateWindowState.NothingSelected)
                {
                    BeginAnimation(EditQuestionOpacityValueProperty, GetOpacityAnimation(EditQuestionOpacityValue, 0.0));
                    return;
                }

                if (modeManager.CurrentState == CreateWindowState.QuestionHighlighted)
                {
                    BeginAnimation(EditQuestionOpacityValueProperty, GetOpacityAnimation(EditQuestionOpacityValue, 0.95));
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
            {
                ValueIsBeingEdited = false;
                e.Handled = true;
            }
        }

        private void PlayOrPauseButtonClicked(object sender, RoutedEventArgs e)
        {
            if (audioVideoPlayer.MediaPlayer == default || audioVideoPlayer.MediaPlayer.Media == default)
                return;

            if (!audioVideoPlayer.MediaPlayer.IsPlaying)
            {
                playPauseIcon.IconType = IconType.Pause;
                audioVideoPlayer.MediaPlayer.Play();
            }
            else
            {
                playPauseIcon.IconType = IconType.Play;
                audioVideoPlayer.MediaPlayer.Pause();
            }
        }

        #region Static methods
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

        private void EnclosingGridMouseEnter(object sender, MouseEventArgs e)
        {
            playPauseIcon.BeginAnimation(OpacityProperty, GetOpacityAnimation(0.0, 0.5));
        }

        private void EnclosingGridMouseLeave(object sender, MouseEventArgs e)
        {
            playPauseIcon.BeginAnimation(OpacityProperty, GetOpacityAnimation(0.5, 0.0));
        }

        private static DoubleAnimation GetOpacityAnimation(double from, double to)
        {
            lock (s_opacityAnimationCache)
            {
                if (!s_opacityAnimationCache.TryGetValue((to, from), out var opacityAnimation))
                {
                    s_opacityAnimationCache.Add((to, from),
                    new DoubleAnimation
                    {
                        From = from,
                        To = to,
                        Duration = TimeSpan.FromMilliseconds(200)
                    });
                }
            }

            return s_opacityAnimationCache[(to, from)];
        }
        #endregion
    }
}
