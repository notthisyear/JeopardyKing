using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using JeopardyKing.GameComponents;
using JeopardyKing.ViewModels;
using JeopardyKing.Windows;
using JeopardyKing.WpfComponents.CustomEventArgs;
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

        public double ProgressBarMarkerPosition
        {
            get => (double)GetValue(ProgressBarMarkerPositionProperty);
            set => SetValue(ProgressBarMarkerPositionProperty, value);
        }
        public static readonly DependencyProperty ProgressBarMarkerPositionProperty = DependencyProperty.Register(
            nameof(ProgressBarMarkerPosition),
            typeof(double),
            typeof(EditQuestionBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region Private fields
        private bool _editQuestionBoxIsToTheLeft;
        private readonly LibVLC _libVlc;
        private static readonly Dictionary<(double to, double from), DoubleAnimation> s_opacityAnimationCache = new();
        #endregion

        private const string LibVlcWindowTitle = "LibVLCSharp.WPF";
        private Window? _libVlcWindow;
        private bool _isClearingMediaQuestion = false;

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
                    ViewModel.NewMediaLoadedEvent += NewMediaLoaded;
                    ViewModel.QuestionTypeChangedEvent += (s, e) => ResetMediaPlayer();
                }

                playPauseIcon.IconType = IconType.Play;
                var mediaPlayer = new VlcMediaPlayer(_libVlc);
                audioVideoPlayer.MediaPlayer = mediaPlayer;
                audioVideoPlayer.MediaPlayer.Playing += MediaPlayerPlayingChanged;
                audioVideoPlayer.MediaPlayer.EndReached += MediaPlayerEndReached;
                audioVideoPlayer.MediaPlayer.PositionChanged += MediaPlayerPositionChanged;
            }
        }

        private void NewMediaLoaded(object? sender, EventArgs e)
        {
            if (ViewModel == default || ViewModel.ModeManager.CurrentlySelectedQuestion == default)
                return;

            if (audioVideoPlayer.MediaPlayer!.Media != default)
                ResetMediaPlayer();

            if (ViewModel.ModeManager.CurrentlySelectedQuestion.Type == QuestionType.Video ||
                ViewModel.ModeManager.CurrentlySelectedQuestion.Type == QuestionType.Audio)
            {
                SetMediaPlayerMediaForQuestion(ViewModel.ModeManager.CurrentlySelectedQuestion, true);
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
            else if (e.PropertyName == nameof(modeManager.CurrentlySelectedQuestion))
            {
                if (audioVideoPlayer.MediaPlayer != default)
                    ResetMediaPlayer();

                if (modeManager.CurrentlySelectedQuestion != default)
                {
                    if (modeManager.CurrentlySelectedQuestion.Type == QuestionType.Video || modeManager.CurrentlySelectedQuestion.Type == QuestionType.Audio)
                        SetMediaPlayerMediaForQuestion(modeManager.CurrentlySelectedQuestion);
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
            if (sender is not Button b || b.Content is not IconBox iconBox)
                return;

            if (audioVideoPlayer.MediaPlayer == default || audioVideoPlayer.MediaPlayer.Media == default)
                return;

            iconBox.IconType = audioVideoPlayer.MediaPlayer.IsPlaying ? IconType.Play : IconType.Pause;
            if (!audioVideoPlayer.MediaPlayer.IsPlaying)
                audioVideoPlayer.MediaPlayer.Play();
            else
                audioVideoPlayer.MediaPlayer.Pause();
        }

        private void MediaPlayerPlayingChanged(object? sender, EventArgs e)
        {
            if (_isClearingMediaQuestion)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (audioVideoPlayer.MediaPlayer == default || audioVideoPlayer.MediaPlayer.Media == default)
                    return;

                // Note: Updating LibVLCSharp properties from the same thread that spawned the event can cause
                //       the app to lock, https://github.com/videolan/libvlcsharp/blob/3.x/docs/best_practices.md
                var mediaPlayer = audioVideoPlayer.MediaPlayer;
                var audioTime = (long)(progressSlider.ProgressMarkerValue * 1000);
                if (mediaPlayer.IsPlaying)
                    Task.Run(() => SetAudioTime(mediaPlayer, mediaPlayer.Media.Duration, audioTime));
            });
        }

        private void MediaPlayerEndReached(object? sender, EventArgs e)
        {
            if (_isClearingMediaQuestion)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (audioVideoPlayer.MediaPlayer == default || audioVideoPlayer.MediaPlayer.Media == default)
                    return;

                playPauseIcon.IconType = IconType.Play;
                var mediaPlayer = audioVideoPlayer.MediaPlayer;
                var clipRangeStart = progressSlider.AreaStart;
                if (!progressSlider.DragActive)
                    progressSlider.ProgressMarkerValue = clipRangeStart;

                Task.Run(() =>
                {
                    mediaPlayer.Stop();
                    SetAudioTime(mediaPlayer, mediaPlayer.Media.Duration, (long)(1000 * clipRangeStart));
                });
            });
        }

        private void MediaPlayerPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            if (_isClearingMediaQuestion)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!progressSlider.DragActive)
                    progressSlider.ProgressMarkerValue = progressSlider.Maximum * e.Position;

                if (progressSlider.ProgressMarkerValue >= progressSlider.AreaEnd)
                    MediaPlayerEndReached(this, EventArgs.Empty);
            });
        }

        private void AudioVideoSetCursorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button)
                return;

            SetAudioOrProgressSliderPosition(progressSlider.AreaStart);
        }

        private void ProgressSliderDragDone(object sender, RoutedEventArgs e)
        {
            if (e is not ProgressMarkerDragDoneEventArgs eventArgs)
                return;

            SetAudioOrProgressSliderPosition(eventArgs.ProgressMarkerValue);
        }

        private void SetAudioOrProgressSliderPosition(double position)
        {
            if (audioVideoPlayer.MediaPlayer == default || audioVideoPlayer.MediaPlayer.Media == default)
                return;

            if (audioVideoPlayer.MediaPlayer.IsPlaying)
                SetAudioTime(audioVideoPlayer.MediaPlayer, audioVideoPlayer.MediaPlayer.Media.Duration, (long)(1000 * position));
            else
                progressSlider.ProgressMarkerValue = position;
        }

        private void SetMediaPlayerMediaForQuestion(Question q, bool isFirstTimeLoaded = false)
        {
            if (ViewModel == default || audioVideoPlayer.MediaPlayer == default || string.IsNullOrEmpty(q.MultimediaContentLink))
                return;

            var media = new Media(_libVlc, q.MultimediaContentLink);
            audioVideoPlayer.MediaPlayer!.Media = media;
            audioVideoPlayer.MediaPlayer.Stop();
            q.VideoOrAudioLengthSeconds = -1;
            Task.Run(async () =>
            {
                await media.Parse(MediaParseOptions.ParseLocal);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ViewModel.ModeManager.CurrentlySelectedQuestion != default)
                    {
                        ViewModel.ModeManager.CurrentlySelectedQuestion.VideoOrAudioLengthSeconds = (int)(media.Duration / 1000.0);
                        if (isFirstTimeLoaded)
                            ViewModel.ModeManager.CurrentlySelectedQuestion.EndVideoOrAudioAtSeconds = ViewModel.ModeManager.CurrentlySelectedQuestion.VideoOrAudioLengthSeconds;
                    }

                    if (_libVlcWindow == default)
                        TrySetVlcWindow();

                    if (_libVlcWindow != default)
                        _libVlcWindow.Visibility = Visibility.Visible;

                    playPauseIcon.IconType = IconType.Play;
                    playPauseIcon.Visibility = Visibility.Visible;
                });
                media.Dispose();
            });
        }

        private void ResetMediaPlayer()
        {
            if (audioVideoPlayer.MediaPlayer == default)
                return;

            _isClearingMediaQuestion = true;

            if (audioVideoPlayer.MediaPlayer.IsPlaying)
                audioVideoPlayer.MediaPlayer.Stop();

            audioVideoPlayer.MediaPlayer.Media?.Dispose();
            audioVideoPlayer.MediaPlayer.Media = default;
            ProgressBarMarkerPosition = 0.0;

            if (_libVlcWindow == default)
                TrySetVlcWindow();

            if (_libVlcWindow != default)
                _libVlcWindow.Visibility = Visibility.Collapsed;
            playPauseIcon.Visibility = Visibility.Collapsed;

            _isClearingMediaQuestion = false;
        }

        private void TrySetVlcWindow()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (LibVlcWindowTitle.Equals(w.Title))
                {
                    _libVlcWindow = w;
                    break;
                }
            }
        }

        #region Static methods
        private static void SetAudioTime(VlcMediaPlayer mediaPlayer, long mediaDurationMs, long newTimeMs)
        {
            mediaPlayer.Time = Math.Min(mediaDurationMs, newTimeMs);
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

        private void EnclosingAudioVideoMouseEnter(object sender, MouseEventArgs e)
        {
            playPauseIcon.BeginAnimation(OpacityProperty, GetOpacityAnimation(0.0, 0.5));
        }

        private void EnclosingAudioVideoMouseLeave(object sender, MouseEventArgs e)
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
