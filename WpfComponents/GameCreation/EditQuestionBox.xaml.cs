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
        private readonly object _mediaPlayerAccessLock = new();

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
                    ViewModel.NewMediaLoadedEvent += (s, e) =>
                    {
                        var question = ViewModel.ModeManager.CurrentlySelectedQuestion;
                        var mediaPlayer = audioVideoPlayer.MediaPlayer;

                        if (question == default || mediaPlayer == default)
                            return;

                        if (question.Type != QuestionType.Audio && question.Type != QuestionType.Video)
                            return;

                        if (mediaPlayer.Media != default)
                            ResetMediaPlayer();

                        Task.Run(() => NewVideoOrAudioMediaLoaded(question));
                    };
                    ViewModel.QuestionTypeChangedEvent += (s, e) => ResetMediaPlayer();
                }

                playPauseIcon.IconType = IconType.Play;

                var mediaPlayer = new VlcMediaPlayer(_libVlc);
                audioVideoPlayer.MediaPlayer = mediaPlayer;
                audioVideoPlayer.MediaPlayer.Playing += MediaPlayerPlayingChanged;
                audioVideoPlayer.MediaPlayer.PositionChanged += MediaPlayerPositionChanged;
                audioVideoPlayer.MediaPlayer.EndReached += MediaPlayerEndReached;
            }
        }

        private void NewVideoOrAudioMediaLoaded(Question question)
        {
            var media = new Media(_libVlc, question.MultimediaContentLink);
            media.Parse(MediaParseOptions.ParseLocal).Wait();
            question.VideoOrAudioLengthSeconds = (int)(media.Duration / 1000.0);
            question.EndVideoOrAudioAtSeconds = question.VideoOrAudioLengthSeconds;

            if (question.Type == QuestionType.Video)
            {
                var videoTrackFound = false;
                MediaTrack videoTrack = default;
                foreach (var track in media.Tracks)
                {
                    if (track.TrackType == TrackType.Video)
                    {
                        videoTrackFound = true;
                        videoTrack = track;
                        break;
                    }
                }

                if (!videoTrackFound)
                    return;

                question.SetImageOrVideoWidthAndHeight((int)videoTrack.Data.Video.Width, (int)videoTrack.Data.Video.Height);
            }
            media.Dispose();

            Application.Current.Dispatcher.Invoke(() => SetMediaPlayerForMediaQuestion(question));
        }

        private void ModeManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not QuestionModeManager modeManager)
                return;

            if (e.PropertyName == nameof(modeManager.CurrentState))
            {
                switch (modeManager.CurrentState)
                {
                    case QuestionVisualState.NothingSelected:
                        if (audioVideoPlayer.MediaPlayer != default)
                            ResetMediaPlayer();
                        BeginAnimation(EditQuestionOpacityValueProperty, GetOpacityAnimation(EditQuestionOpacityValue, 0.0));
                        break;

                    case QuestionVisualState.QuestionHighlighted:
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
                        break;

                    case QuestionVisualState.QuestionSelected:
                        if (modeManager.CurrentlySelectedQuestion == default)
                            return;

                        if (modeManager.CurrentlySelectedQuestion.Type == QuestionType.Video || modeManager.CurrentlySelectedQuestion.Type == QuestionType.Audio)
                            SetMediaPlayerForMediaQuestion(modeManager.CurrentlySelectedQuestion);
                        break;
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
            lock (_mediaPlayerAccessLock)
            {
                if (!audioVideoPlayer.MediaPlayer.IsPlaying)
                    audioVideoPlayer.MediaPlayer.Play();
                else
                    audioVideoPlayer.MediaPlayer.Pause();
            }
        }

        private void MediaPlayerPlayingChanged(object? sender, EventArgs e)
        {
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

        private void MediaPlayerPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!progressSlider.DragActive)
                    progressSlider.ProgressMarkerValue = progressSlider.Maximum * e.Position;

                if (progressSlider.ProgressMarkerValue >= progressSlider.AreaEnd)
                    MediaPlayerEndReached(default, EventArgs.Empty);
            });
        }

        private void MediaPlayerEndReached(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (audioVideoPlayer.MediaPlayer == default || audioVideoPlayer.MediaPlayer.Media == default)
                    return;

                playPauseIcon.IconType = IconType.Play;
                var clipRangeStart = progressSlider.AreaStart;
                if (!progressSlider.DragActive)
                    progressSlider.ProgressMarkerValue = clipRangeStart;

                var mediaPlayer = audioVideoPlayer.MediaPlayer;
                Task.Run(() =>
                {
                    lock (_mediaPlayerAccessLock)
                    {
                        mediaPlayer.Stop();
                        SetAudioTime(mediaPlayer, mediaPlayer.Media.Duration, (long)(1000 * clipRangeStart));
                    }
                });
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
            {
                lock (_mediaPlayerAccessLock)
                    SetAudioTime(audioVideoPlayer.MediaPlayer, audioVideoPlayer.MediaPlayer.Media.Duration, (long)(1000 * position));
            }
            else
            {
                progressSlider.ProgressMarkerValue = position;
            }
        }

        private void SetMediaPlayerForMediaQuestion(Question question)
        {
            if (audioVideoPlayer.MediaPlayer == default || string.IsNullOrEmpty(question.MultimediaContentLink))
                return;

            lock (_mediaPlayerAccessLock)
            {
                var media = new Media(_libVlc, question.MultimediaContentLink);
                audioVideoPlayer.MediaPlayer.Media = media;
                media.Dispose();
            }

            SetVlcWindowVisibility(Visibility.Visible);
            playPauseIcon.IconType = IconType.Play;
            playPauseIcon.Visibility = Visibility.Visible;
        }

        private void ResetMediaPlayer()
        {
            if (audioVideoPlayer.MediaPlayer == default)
                return;

            lock (_mediaPlayerAccessLock)
            {
                audioVideoPlayer.MediaPlayer.Stop();
                if (audioVideoPlayer.MediaPlayer.Media != default)
                    audioVideoPlayer.MediaPlayer.Media?.Dispose();
            }

            ProgressBarMarkerPosition = 0.0;
            SetVlcWindowVisibility(Visibility.Collapsed);
            playPauseIcon.Visibility = Visibility.Collapsed;
        }

        private void SetVlcWindowVisibility(Visibility visibility)
        {
            if (_libVlcWindow == default)
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

            if (_libVlcWindow != default)
                _libVlcWindow.Visibility = visibility;
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
